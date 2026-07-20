# Editor de Clipboard

App minimo em Electron para editar rapidamente o texto atual do clipboard a partir da bandeja do Windows.

## Como rodar em desenvolvimento

```bash
pnpm install
pnpm start
```

O programa nao abre janela. Ele fica nos icones ocultos do Windows, e o menu aparece ao clicar no icone.

## Acoes

- `Colocar uma virgula no final de cada linha.`: separa os itens por linha e adiciona `,` ao fim de cada item, exceto o ultimo.
- `Colocar aspas simples em cada linha.`: separa os itens por linha e transforma cada item em `'item'`.
- `Transformar em lista para SQL`: separa os itens por linha e gera uma lista SQL com cada item em uma linha.

Os separadores aceitos na entrada sao espacos, virgulas e quebras de linha.

## Gerar instalador

```bash
pnpm run dist
```

O instalador sera criado na pasta `dist`.

## Gerar versao portable

```bash
pnpm run portable
```

A versao portable tambem sera criada na pasta `dist`.
