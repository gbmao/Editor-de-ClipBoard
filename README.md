# Editor de Clipboard

App simples para Windows que fica na bandeja do sistema e transforma rapidamente o texto atual do clipboard.

Agora o app e feito em C# com Windows Forms, sem Electron. A publicacao leve gera um executavel de aproximadamente 163 KB, dependendo do ambiente de build.

## Requisito

Para manter o app pequeno, o instalador nao inclui o runtime do .NET.

A maquina precisa ter o **.NET 8 Desktop Runtime** instalado.

Download oficial:

```text
https://dotnet.microsoft.com/download/dotnet/8.0
```

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

- .NET SDK 8 ou superior
- Windows

Compilar:

```bash
dotnet build src/EditorDeClipboard/EditorDeClipboard.csproj -c Release
```

Rodar em desenvolvimento:

```bash
dotnet run --project src/EditorDeClipboard/EditorDeClipboard.csproj
```

## Publicar executavel leve

```bash
dotnet publish src/EditorDeClipboard/EditorDeClipboard.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:DebugType=none -p:DebugSymbols=false -o publish
```

O executavel sera criado em:

```text
publish/EditorDeClipboard.exe
```

## Gerar instalador local

O instalador usa Inno Setup.

Depois de publicar o executavel em `publish/`, rode:

```powershell
Push-Location installer
iscc EditorDeClipboard.iss
Pop-Location
```

O instalador sera criado na pasta `dist`.

## Publicar uma nova Release

O instalador e gerado automaticamente pelo GitHub Actions quando uma tag com prefixo `v` e enviada para o GitHub.

Exemplo:

```bash
git tag v0.2.0
git push origin v0.2.0
```

O workflow `Build and Release` vai:

- instalar o .NET SDK
- compilar o projeto em Release
- publicar o executavel leve
- instalar o Inno Setup
- gerar o instalador Windows
- criar uma Release no GitHub
- anexar o `.exe` do instalador nessa Release

Para rodar o build sem criar Release, use a aba `Actions` no GitHub e execute o workflow manualmente. Nesse caso, o instalador fica disponivel como artifact chamado `windows-installer`.

Se a criacao da Release falhar por permissao, habilite `Read and write permissions` em:

```text
Settings > Actions > General > Workflow permissions
```

## Estrutura

```text
src/EditorDeClipboard/                 app Windows Forms
installer/EditorDeClipboard.iss        instalador Inno Setup
.github/workflows/release.yml          workflow para gerar instalador e Release
```
