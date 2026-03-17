# 📚 Acervo Leitor

Sistema completo de gestão de biblioteca, desenvolvido em **ASP.NET Core**, com autenticação, controle de empréstimos e modo dual de execução (**DEV e DEMO**).

---

## 🚀 Tecnologias Utilizadas

- ASP.NET Core  
- .NET 9  
- Entity Framework Core  
- ASP.NET Identity (Autenticação e Roles)  
- SQL Server  
- SQLite (Modo Demo)  
- Bootstrap  
- Razor Views  

---

## 🎯 Objetivo do Sistema

O **Acervo Leitor** foi desenvolvido para:

- 📖 Gerenciar livros  
- 👨‍🎓 Controlar alunos  
- 🔄 Registrar empréstimos  
- 📅 Controlar devoluções  
- 🚫 Bloquear empréstimos inválidos  
- 🔐 Garantir segurança com login obrigatório  
- 💻 Permitir modo demonstração offline  

---

## 🔐 Sistema de Autenticação

O projeto utiliza **ASP.NET Identity** com:

- Login obrigatório
- Sistema de Roles
- Role padrão: **Admin**
- Usuário demo criado automaticamente no modo demo

---

## 🧪 Modo de Execução

O sistema possui dois modos automáticos:

### 🧠 Modo Desenvolvimento (Visual Studio)

- Banco: **SQL Server**
- Ambiente: **Development**
- Usado ao rodar pelo Visual Studio ou `dotnet run`

---

### 📦 Modo Demo (EXE Publicado)

- Banco: **SQLite**
- Arquivo: `demo.db`
- Ativado automaticamente quando:
  - Está rodando como `.exe`
  - Existe o arquivo `demo.db` na pasta do executável

🔹 Ideal para demonstrações offline.

**Usuário criado automaticamente:**


Email: demo@demo.com

Senha: Demo123
Role: Admin


---

## 📂 Estrutura de Publicação

Após executar:

```bash
dotnet publish -c Release -r win-x64 --self-contained true

O sistema será gerado em:

bin/Release/net9.0/win-x64/publish

Para o modo demo funcionar, o arquivo:

demo.db

deve estar dentro da pasta publish.

▶️ Como Executar
🔹 No Visual Studio

Basta pressionar F5.

🔹 Pelo Executável

Ir até a pasta:

publish

Executar:

Acervo Leitor.exe
🛠 Como Publicar Nova Versão

Dentro da pasta do projeto:

dotnet clean
dotnet publish -c Release -r win-x64 --self-contained true
🔹 Versão compacta (Single File):
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true
🗄 Banco de Dados
🔵 Produção

SQL Server

Configurado em appsettings.json

🟢 Demo

SQLite

Arquivo: demo.db

Criado automaticamente no primeiro uso

🔒 Regras de Negócio

Livros com empréstimos ABERTO ou ATRASO não podem ser reutilizados

Controle de status de empréstimo

Sistema seguro de exclusão lógica

Acesso protegido por autenticação obrigatória

📌 Requisitos

.NET 9 SDK

SQL Server (modo desenvolvimento)

Windows (para execução do .exe publicado)

👨‍💻 Autor

Luahr Veiga

📄 Licença

Uso interno / Educacional.
Distribuição mediante autorização.
