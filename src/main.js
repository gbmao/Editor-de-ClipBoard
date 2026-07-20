const { app, clipboard, Menu, Notification, Tray, nativeImage } = require('electron');

let tray = null;

const appName = 'Editor de Clipboard';
const lineBreak = '\n';

function paintPixel(buffer, size, x, y, color) {
  if (x < 0 || y < 0 || x >= size || y >= size) {
    return;
  }

  const [r, g, b, a = 255] = color;
  const index = (y * size + x) * 4;

  buffer[index] = b;
  buffer[index + 1] = g;
  buffer[index + 2] = r;
  buffer[index + 3] = a;
}

function fillRect(buffer, size, left, top, width, height, color) {
  for (let y = top; y < top + height; y += 1) {
    for (let x = left; x < left + width; x += 1) {
      paintPixel(buffer, size, x, y, color);
    }
  }
}

function createTrayIcon() {
  const size = 16;
  const buffer = Buffer.alloc(size * size * 4, 0);

  fillRect(buffer, size, 3, 2, 10, 12, [24, 31, 42]);
  fillRect(buffer, size, 4, 3, 8, 10, [245, 247, 250]);
  fillRect(buffer, size, 6, 1, 4, 3, [37, 99, 235]);
  fillRect(buffer, size, 5, 5, 6, 1, [20, 184, 166]);
  fillRect(buffer, size, 5, 8, 6, 1, [20, 184, 166]);
  fillRect(buffer, size, 5, 11, 4, 1, [20, 184, 166]);

  const image = nativeImage.createFromBitmap(buffer, {
    height: size,
    scaleFactor: 1,
    width: size,
  });

  image.setTemplateImage(false);

  return image;
}

function showNotification(body) {
  if (!Notification.isSupported()) {
    return;
  }

  new Notification({
    title: appName,
    body,
    silent: true,
  }).show();
}

function getClipboardItems(text) {
  const normalizedText = text
    .replace(/[,\s]+/g, lineBreak)
    .replace(/^\n+|\n+$/g, '');

  if (normalizedText.length === 0) {
    return [];
  }

  return normalizedText.split(lineBreak);
}

function addCommaToEachItem(items) {
  return items
    .map((item, index) => (index === items.length - 1 ? item : `${item},`))
    .join(lineBreak);
}

function quoteEachItem(items) {
  return items
    .map((item) => `'${item}'`)
    .join(lineBreak);
}

function turnIntoSqlList(items) {
  const quotedItems = items.map((item) => `'${item.replaceAll("'", "''")}'`);
  return `(${lineBreak}${quotedItems.join(`,${lineBreak}`)}${lineBreak})`;
}

function transformClipboardText(text, action) {
  const items = getClipboardItems(text);

  if (items.length === 0) {
    return '';
  }

  switch (action) {
    case 'comma':
      return addCommaToEachItem(items);
    case 'quote':
      return quoteEachItem(items);
    case 'sql-list':
      return turnIntoSqlList(items);
    default:
      throw new Error('Acao invalida.');
  }
}

function runClipboardAction(action, actionLabel) {
  try {
    const output = transformClipboardText(clipboard.readText(), action);
    clipboard.writeText(output);
    showNotification(`${actionLabel} aplicado ao clipboard.`);
  } catch (error) {
    showNotification(`Nao foi possivel alterar o clipboard: ${error.message}`);
  }
}

function createTrayMenu() {
  return Menu.buildFromTemplate([
    {
      label: 'Colocar uma vírgula no final de cada linha.',
      click: () => runClipboardAction('comma', 'Virgula no final de cada linha'),
    },
    {
      label: 'Colocar aspas simples em cada linha.',
      click: () => runClipboardAction('quote', 'Aspas simples em cada linha'),
    },
    {
      label: 'Transformar em lista para SQL',
      click: () => runClipboardAction('sql-list', 'Lista para SQL'),
    },
    { type: 'separator' },
    {
      label: 'Fechar',
      click: () => app.quit(),
    },
  ]);
}

app.setName(appName);

if (process.platform === 'win32') {
  app.setAppUserModelId('com.editor-de-clipboard.app');
}

app.whenReady().then(() => {
  const trayMenu = createTrayMenu();

  tray = new Tray(createTrayIcon());
  tray.setToolTip(appName);
  tray.setContextMenu(trayMenu);

  tray.on('click', () => {
    tray.popUpContextMenu(trayMenu);
  });
});
