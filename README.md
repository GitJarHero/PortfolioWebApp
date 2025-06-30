# 🚀 PortfolioWebApp

**PortfolioWebApp** Blazor Server Web Application zum Chatten in Echtzeit. 

Verwendete Technologien:
.NET 8 + MudBlazor + PostgreSQL.

---

## 📦 Installation & Start

1. **Navigation ins Verzeichnis**
PortfolioWebApp/Dev/Containers/

2. **Script ausführbar machen (falls nötig)**
chmod +x build_and_deploy

3. **App bauen und starten**
./build_and_deploy

4. **Aufruf im Browser**

[http://localhost:5120](http://localhost:5120)

---

## 🔐 Login

- Beispiel-User: `test123`  
- Passwort: `test123`  
- Der User _test123_ hat bereits initiale Daten (Chats, Freunde etc.) und ist mit allen anderen Usern befreundet.
- **Hinweis:** Für alle User entspricht das Passwort jeweils dem Usernamen.

---

## 📝 Registrierung

Es können auch eigene Accounts registriert werden!  
Beachten: Neue Freunde müssen dann **manuell** gesucht und hinzugefügt werden.

---

## 💬 Chat starten

- Öffne die **Freundesliste**.
- Klicke auf das **Chat-Icon** neben dem gewünschten Freund, um einen neuen Chat zu starten.

---

## 📄 Technische Diagramme

Detaillierte Dokumentation und Kommunikationsdiagramme findest du unter:

PortfolioWebApp/Dev/Documentation/


_(PDF-Dateien)_

---

## 🛑 App stoppen & neustarten


**Verzeichnis:**  
`PortfolioWebApp/Dev/Containers/`


### Stoppen

- Komplett:
./stop

- Nur Datenbank:
./stop db

- Nur WebApp:
./stop webapp


### Neustarten

- Komplett:
./restart

- Nur Datenbank:
./restart db

- Nur WebApp:
./restart webapp
