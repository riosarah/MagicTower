
Lies dir die Instructions_FE.md sowie die *Flower_Shop_API_Documentation.md* durch.

 Du sollst jetzt auf basis des schon erstellten Angularprojekts einen Plan für die Umsetzung dieser Anweisungen erstellen.

### Teil C: Angular Frontend (35 Punkte)

Erstellen Sie einen Angular Client für die Lagerverwaltung des *FlowerShops*.

1. **Projektstruktur & Setup**
   - Erstellen Sie ein neues Angular-Projekt.
   - Konfigurieren Sie das Routing für die verschiedenen Ansichten.
   - Binden Sie Angular Material oder Bootstrap für das Styling ein.

2. **Artikel-Service**
   - Erstellen Sie einen `ArtikelService` mit HttpClient.
   - Implementieren Sie Methoden für CRUD-Operationen:
     - `getAll(): Observable<Artikel[]>`
     - `getById(artikelNr: string): Observable<Artikel>`
     - `create(artikel: Artikel): Observable<Artikel>`
     - `update(artikel: Artikel): Observable<Artikel>`
     - `delete(artikelNr: string): Observable<void>`

3. **Item'liste mit Filterung**
   - Erstellen Sie eine `ItemListComponent` mit tabellarischer Darstellung.
   - Implementieren Sie Filtermöglichkeiten nach:
     - Kategorie (Dropdown)
     - Lieferant (Dropdown)
     - Suchfeld für Produktname
   - Fügen Sie Buttons für Bearbeiten und Löschen hinzu.

4. **Item-Formular**
   - Erstellen Sie eine `ArtikelFormComponent` für Neuanlage und Bearbeitung.
   - Verwenden Sie Reactive Forms mit folgenden Validierungen:
     - Artikel-Nr.: Pflichtfeld, Pattern `B[0-9]{4}`
     - Produkt: Pflichtfeld, min. 3 Zeichen
     - Einkaufspreis/Verkaufspreis: Pflichtfeld, positiver Wert
     - Bestand: Pflichtfeld, Ganzzahl >= 0
     - Haltbar bis: Pflichtfeld, Datum in der Zukunft
   - Zeigen Sie Validierungsfehler benutzerfreundlich an.

5. **Dashboard-Ansicht**
   - Erstellen Sie eine `DashboardComponent` mit Übersicht:
     - Gesamtanzahl Artikel
     - Artikel mit niedrigem Bestand (< 50)
     - Artikel mit bald ablaufendem Haltbarkeitsdatum
   - Verwenden Sie Cards oder Kacheln für die Darstellung.

---

### Teil D: n8n-Workflow Integration (20 Punkte)

Erstellen Sie einen n8n-Workflow **"FlowerStoreAlert"** zur automatischen Lagerüberwachung.

1. **Webhook-Trigger einrichten** (5 Punkte)
   - Erstellen Sie einen Webhook-Node mit dem Namen `FlowerStoreChanged`.
   - Der Webhook empfängt JSON-Daten mit den aktuellen Lagerbeständen.
   - Konfigurieren Sie die HTTP-Methode (POST) und den Pfad.

2. **Excel-Integration** (7 Punkte)
   - Erstellen Sie eine Excel-Datei `FlowerStore.xlsx` mit folgenden Spalten:
     - Artikel-Nr.
     - Produkt
     - Aktueller Bestand
     - Mindestbestand
     - Email-Adresse (Einkäufer)
     - Email versendet am (Datum)
   - Konfigurieren Sie einen Excel-Node zum Schreiben/Aktualisieren der Lagerstände.
   - Der Einkäufer kann Mindestbestand und Email-Adresse manuell in der Excel-Datei pflegen.

3. **Bestandsprüfung & Benachrichtigung** (8 Punkte)
   - Implementieren Sie eine Logik zur Prüfung: `Aktueller Bestand < Mindestbestand`
   - Bei Unterschreitung des Mindestbestands:
     - Senden Sie eine Email an die hinterlegte Email-Adresse des Einkäufers.
     - Die Email soll enthalten: Artikel-Nr., Produktname, aktueller Bestand, Mindestbestand.
   - Verwenden Sie einen IF-Node für die Bedingungsprüfung und einen Email-Node für den Versand.

**Workflow-Ablauf:**
```
[Webhook: FlowerStoreChanged] → [Excel: Lesen FlowerStore.xlsx] → [Merge/Update Bestände] 
    → [Excel: Schreiben FlowerStore.xlsx] → [IF: Bestand < Mindestbestand?] 
    → [Email: Benachrichtigung an Einkäufer]
```

---


### Angular-spezifische Hinweise

- Verwenden Sie Angular CLI für die Projekterstellung: `ng new flowershop-client`
- Generieren Sie Komponenten mit: `ng generate component <name>`
- Generieren Sie Services mit: `ng generate service <name>`
- Die Backend-API läuft auf `http://localhost:7074/api`
- Konfigurieren Sie CORS im Backend oder verwenden Sie einen Proxy (`proxy.conf.json`).
- Nutzen Sie Interfaces für die Typisierung der Datenmodelle.


### 5. Angular-Komponenten

1. Erstelle für alle Entitäten die List-Komponente
   - Die List-Komponenten wurden vom Generator erstellt und befinden sich im Ordner 'src/app/pages/entities/'
   - Immer mit HTML-Templates in einer separaten Datei arbeiten.
   - Immer mit CSS-Templates in einer separaten Datei arbeiten.
   - **List Components anpassen:**
   - Suchfelder konfigurieren
   - Anzeige-Properties definieren
   - Filter/Sorting implementieren (falls benötigt)
2. Erstelle für alle List-Komponenten das Routing in `app-routing.module.ts`.
3. Trage alle List-Komponenten in das Dashboard für die Navigation ein.
4. **Routing hinzufügen:**
   - Routes in `app.routes.ts` eintragen
   - Navigation im Dashboard ergänzen
5. Erstelle für alle Entitäten die Edit-Komponente
   - Die Edit-Komponenten wurden vom Generator erstellt und befinden sich im Ordner 'src/app/components/entities/'
   - Verweise auf andere Entities als Dropdowns umsetzen.
   - Immer mit HTML-Templates in einer separaten Datei arbeiten.
   - Immer mit CSS-Templates in einer separaten Datei arbeiten.
   - **Edit Components anpassen:**
   - Formularfelder basierend auf Property-Typen generieren
   - Dropdowns für Foreign Keys implementieren
   - Enum-Selects hinzufügen
   - Date-Picker konfigurieren
6. Setze Master-Detail-Ansichten um (falls benötigt)
8. Erstelle die Html files für die Komponenten im responsive card style.
   - Verwende Bootstrap-Klassen für Layout und Styling.
   - Achte auf Barrierefreiheit (ARIA-Labels, Tastaturnavigation).
7. Übersetzungen `de.json` und `en.json` für alle neuen Labels ergänzen
   - Format: `ENTITYNAME_LIST.*` und `ENTITYNAME_EDIT.*`

### Styling

Das Globale Stylingeinstellungen sollen unter der style.css AngularApp\src\styles.css
gespeichert werden.
Für die einzelnen Komponenten, falls Abänderungen notwendig sind,
werden diese in der css Einstellung der Komponente gespeichert.

### Internationalisierung

- Alle Labels in i18n-Dateien
- Format: `ENTITYNAME_LIST.TITLE`
- Unterstützung für DE/EN

---

### Übersetzungen

#### Deutsche und englische Übersetzungen

Hinzufügen von Übersetzungen für die neue Entität in die Datei `de.json` und `en.json` :

```json
{
  "ENTITYNAME_LIST": {
    "TITLE_PLURAL": "EntityNames",
    "BACK": "Zurück",
    "SEARCH_PLACEHOLDER": "Suchen...",
    "ADD": "Hinzufügen",
    "NO_NAME": "Kein Name",
    "UPDATE": "Bearbeiten",
    "DELETE": "Löschen",
    "NO_RESULTS": "Keine Ergebnisse gefunden"
  },
  "ENTITYNAME_EDIT": {
    "TITLE_EDIT": "EntityName bearbeiten",
    "TITLE_CREATE": "Neue EntityName erstellen",
    "LABEL_NAME": "Name",
    "LABEL_DESCRIPTION": "Beschreibung",
    "LABEL_RELATED": "Verwandte Entität",
    "SELECT_RELATED": "Verwandte Entität auswählen"
  }
}
```

#### Checkliste:

- [ ] Frontend: List Component zeigt Daten
- [ ] Frontend: Create-Modal öffnet und speichert
- [ ] Frontend: Edit-Modal ist als standalone Komponente definiert, lädt Daten und speichert
- [ ] Frontend: Delete funktioniert mit Bestätigung
- [ ] Frontend: Suche filtert korrekt
- [ ] Übersetzungen: DE/EN wechseln funktioniert
- [ ] Validierung: Frontend- und Backend-Validierung greifen
- [ ] Relations: Dropdowns zeigen Daten, Foreign Keys werden gespeichert
