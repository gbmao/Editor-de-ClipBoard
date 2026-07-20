# Editor de Clipboard

App simples para Windows que fica na bandeja do sistema e transforma rapidamente o texto atual do clipboard.

## Instalar

Baixe o instalador mais recente na pagina de Releases do GitHub e execute o arquivo:

```text
Editor de Clipboard Setup x.y.z.exe
```

O app nao abre janela. Depois de instalado, ele fica nos icones ocultos do Windows. Clique no icone para abrir o menu de acoes.

Observacao: o instalador ainda nao e assinado. O Windows pode mostrar um aviso de seguranca/SmartScreen na primeira execucao.

## Acoes

- `Colocar uma virgula no final de cada linha.`: separa os itens por linha e adiciona `,` ao fim de cada item, exceto o ultimo.
- `Colocar aspas simples em cada linha.`: separa os itens por linha e transforma cada item em `'item'`.
- `Transformar em lista para SQL`: separa os itens por linha e gera uma lista SQL com cada item em uma linha.

Os separadores aceitos na entrada sao espacos, virgulas e quebras de linha.

Exemplo de entrada:

```text
item1, item2 item3
```

Exemplo de lista SQL:

```sql
(
'item1',
'item2',
'item3'
)
```

## Desenvolvimento

Requisitos:

- Node.js
- pnpm

Instale as dependencias:

```bash
pnpm install
```

Rode em desenvolvimento:

```bash
pnpm start
```

Cheque a sintaxe:

```bash
pnpm run check
```

## Gerar instalador local

```bash
pnpm run dist
```

O instalador sera criado na pasta `dist`.

## Gerar versao portable local

```bash
pnpm run portable
```

A versao portable tambem sera criada na pasta `dist`.

## Publicar uma nova Release

O instalador e gerado automaticamente pelo GitHub Actions quando uma tag com prefixo `v` e enviada para o GitHub.

Exemplo:

```bash
git tag v0.1.1
git push origin v0.1.1
```

O workflow `Build and Release` vai:

- instalar as dependencias com `pnpm`
- rodar `pnpm run check`
- gerar o instalador Windows com `electron-builder`
- criar uma Release no GitHub
- anexar o `.exe` da instalacao nessa Release

Para rodar o build sem criar Release, use a aba `Actions` no GitHub e execute o workflow manualmente. Nesse caso, o instalador fica disponivel como artifact chamado `windows-installer`.

Se a criacao da Release falhar por permissao, habilite `Read and write permissions` em:

```text
Settings > Actions > General > Workflow permissions
```

## Estrutura

```text
src/main.js                     logica principal do app Electron
.github/workflows/release.yml   workflow para gerar instalador e Release
package.json                    scripts e configuracao do electron-builder
```
