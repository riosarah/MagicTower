# GitHub Copilot Instructions für ProjectManagement

## Projektübersicht

Dies ist ein Basis-Template für die Erstellung der Anwendung mit Code-Generierung:
- **Backend**: .NET 8.0 mit Entity Framework Core
- **Frontend**: Angular 18 mit Bootstrap und standalone Komponenten
- **Code-Generierung**: Template-gesteuerte Erstellung aller CRUD-Operationen
- **Architektur**: Clean Architecture mit strikter Trennung von manuellen und generierten Code

## Entity-Entwicklung

Die Entitäten werden immer mit englischen Bezeichner benannt.

### Dateistruktur
- **Stammdaten**: `ProjectManagement.Logic/Entities/Data/`
- **Anwendungsdaten**: `ProjectManagement.Logic/Entities/App/`
- **Account**: `ProjectManagement.Logic/Entities/Account/`

### Entity Template

- Erstelle die Klasse mit dem Modifier *public* und *partial*.
- Die Klasse erbt von `EntityObject`.  
- Dateiname: **EntityName.cs**.  

Beispielformat:
```csharp
//@Ai_Code
namespace AiKita_BE.Logic.Entities
{
    [Table("Entity")]
    [Index(nameof(propertyOne), isUnique = true, Name = "EntityName_Index")]
    public partial class EntityName : EntityObject
    {
      #region properties

      [MaxLength(Number)]
      [Required]   
      string AutoNameOne {get;set;} = string.empty;
      [MaxLength(Number)]
      public string[] AutoArray {get;set;} = Array.Empty<string>();

      #endregion properties

      #region navigationalProperties

      [ForeignKey("OtherTableName")]
      public IdType OtherTableId {get;set;}

      #endregion navigationalProperties

      #region constructor

      public EntityName(){}

      #endregion constructor

      #region overrides

      public override ToString(){
      return $"{AutoNameOne} {AutoNameTwo}...."
      }
      #endregion overrides
    }
}

```



## Struktur für Validierungsklassen

- Lege eine separate *partial* Klasse für die Validierung im **gleichen Namespace** wie die Entität an.  
- Die Klasse implementiert `IValidatableEntity`.  
- Dateiname: **EntityName.Validation.cs**.  
- Erkennbare Validierungsregeln aus der Beschreibung müssen implementiert werden.

BeispielFormat:
```csharp

namespace AiKita_BE.Logic.Entities
{
    public partial class EntityName :IValidatableEntity
    {
        private const int DefinitionLength = 4;       

        public void Validate(IContext context, EntityState entityState)
        {
            var errors = new List<string>();


            if (string.IsNullOrEmpty(AutoNameOne[0]))
                errors.Add($"{nameof(AutoNameOne)} Definition must not be empty");
            if (AutoNameTwo.Length < DefinitionLength)
                errors.Add($"{nameof(AutoNameTwo)} must be longer than {DefinitionLength} letters");
            if (OtherTableId == 0 || OtherTableId < 0)
            {
                errors.Add($"{nameof(OtherTableId)} reference must not be null or negative.");
            }

            if (errors.Any())
                throw new ValidationException(string.Join(" | ", errors));
        }
    }
}


```


## Using-Regeln

- `using System` wird **nicht** explizit angegeben.

## Entity-Regeln

- Kommentar-Tags (`/// <summary>` usw.) sind für jede Entität erforderlich.  
- `ProjectManagement.Logic` ist fixer Bestandteil des Namespace.  
- `[.SubFolder]` ist optional und dient der Strukturierung.

## Property-Regeln

- Primärschlüssel `Id` wird von `EntityObject` geerbt.  
- **Auto-Properties**, wenn keine zusätzliche Logik benötigt wird.  
- **Full-Properties**, wenn Lese-/Schreiblogik erforderlich ist.  
- Für Id-Felder: Typ `IdType`.  
- Bei Längenangabe: `[MaxLength(n)]`.  
- Nicht-nullable `string`: `= string.Empty`.  
- Nullable `string?`: keine Initialisierung.

## Navigation Properties-Regeln

- In der Many-Entität: `EntityNameId`.  
- Navigation Properties immer vollqualifiziert:  
  `ProjectName.Entities.EntityName EntityName`  
- **1:n**:

```csharp
  public List<Type> EntityNames { get; set; } = [];
```  

- **1:1 / n:1**:  

```csharp
  Type? EntityName { get; set; }
```


- **n:m**:  
  Frage wegen der gewünschten Implementierung nach.

## Dokumentation

- Jede Entität und Property erhält englische XML-Kommentare.

**Beispiel:**

```csharp
/// <summary>
/// Name of the entity.
/// </summary>
public string Name { get; set; } = string.Empty;

```

### 3. Code-Marker System

- `//@AiCode` - Generierter Code, nicht bearbeiten
- `//@GeneratedCode` - Zeigt an, dass dieser Code vom Generator generiert wurde und bei der nächsten Generierung überschrieben wird.
- `//@CustomCode` - Falls in einer generierten Datei (@GeneratedCode) eine Änderung erfolgt, dann wird der Label @GeneratedCode zu @CustomCode geändert. Damit wird verhindert, dass der Code vom Generator überschrieben wird.
- `#if GENERATEDCODE_ON` - Conditional Compilation für Features




## TemplateTools.ConApp - Komplette Kommando-Referenz

### Übersicht der AppArg-Parameter

Format: `dotnet run --project TemplateTools.ConApp -- AppArg=X,Y,Z,W`

| Kommando | Beschreibung | Wann verwenden |
|----------|--------------|----------------|
| `3,2,x,x` | Authentifizierung ein/ausschalten | Zu Beginn des Projekts |
| `4,9,x,x` | Vollständige Code-Generierung | Nach jeder Entity-Änderung |
| `4,7,x,x` | Generierte Klassen löschen | Vor größeren Entity-Änderungen |



## Konventionen

### Naming

- Entities: PascalCase, Englisch
- Properties: PascalCase mit XML-Dokumentation
- Navigation Properties: Vollqualifiziert

### Validierung

- Keine Validierung für Id-Felder
- BusinessRuleException für Geschäftsregeln
- Async-Pattern mit RejectChangesAsync()





## Entwicklungs-Workflow

## Von Entity zu vollständiger UI - Kompletter Workflow

### Phase 1: Entity-Definition bestätigen

1. **Benutzer beschreibt Entität(en)** im definierten Format
2. **Copilot erstellt Entity-Klasse(n)** nach Template
3. **Copilot erstellt Validierungs-Klasse(n)**
4. **Benutzer bestätigt Entity-Modell** → Weiter zu Phase

Wenn der Benutzer Entitäten beschreibt, folge diesem Ablauf:

#### ✅ Phase 1: Anforderungen klären

- [ ] Entitätsbeschreibung vollständig? (Name, Type, Properties, Relations, Validation)
- [ ] Authentifizierung benötigt? (Ja/Nein)
- [ ] Master-Detail-Ansicht gewünscht? (Ja/Nein)
- [ ] Import-Daten vorhanden? (CSV-Dateien)
- [ ] Spezielle UI-Anforderungen? (z.B. Filtering, Sorting, Pagination)

#### ✅ Phase 2: Backend erstellen

1. **Entity-Klassen erstellen:**
   - Datei: `ProjectName.Logic/Entities/{Data|App|Account}/EntityName.cs`
   - Template verwenden (siehe "Entity Template")
   - XML-Dokumentation hinzufügen
   


2. **Benutzer-Bestätigung einholen:**

3. **Datenbank erstellen:**
   - `dotnet run --project ProjectName.ConApp -- AppArg=1,2,x`
   - Datenimport in der Conapp hinzufügen. 
   - Sicherstellen, dass die csv dateien in den zielordner kopiert werden.(Copy if newer) 

### 1. Authentifizierung einstellen

1. Die Standard-Einstellung ist ohne Authentifizierung. 
2. Frage den Benutzer, ob Authentifizierung benötigt wird.
3. Authentifizierung ausführen: `dotnet run --project TemplateTools.ConApp -- AppArg=3,2,x,x`

### 2. Entity erstellen

1. Entity-Klasse in `Logic/Entities/{Data|App}/` erstellen
2. Validierung in separater `.Validation.cs` Datei
3. Das Entity-Modell mit dem Benutzer abklären und bestätigen lassen.

### 2. Code-Generierung

1. Code-Generierung ausführen: `dotnet run --project TemplateTools.ConApp -- AppArg=4,9,x,x`

### 3. Daten-Import

1. Überprüfen ob CSV Dateien vorhanden sind. Wenn nicht sollen Beispieldateien generiert werden.
2. CSV-Datei in `ConApp/data/entityname_set.csv` erstellen
3. Einstellen, dass die CSV-Datei ins Ausgabeverzeichnis kopiert wird
4. Import-Logic in `StarterApp.Import.cs` hinzufügen
5. Console-App ausführen und Import starten
6. DatenImport soll als eigene Auswahlmöglichkeit in der ConApp verfügbar sein.

### 4. Datenbank erstellen und Import starten

1. Code-Generierung ausführen: `dotnet run --project ProjectManagement.ConApp -- AppArg=1,2,x`
    - Überprüfung ob die entsprechenden Tabellen korrekt angelegt worden sind.

Beispiel für StarterApp.Import.cs:

 ```csharp
 //@CustomCode
using FlowerShop.Logic.Entities.Data;
using FlowerShop.Logic.Entities.App;
using System.Globalization;

namespace FlowerShop.ConApp.Apps
{
    /// <summary>
    /// Partial class for StarterApp containing CSV import logic.
    /// </summary>
    public partial class StarterApp
    {
        #region constants
        private const string CsvFilePath = "Data/dataset_FlowerWarehouse.csv";
        #endregion constants

        #region partial method implementations
        /// <summary>
        /// Adds additional menu items after the standard menu items.
        /// </summary>
        partial void AfterCreateMenuItems(ref int menuIdx, List<MenuItem> menuItems)
        {
            var idx = menuIdx;
            menuItems.Add(new MenuItem
            {
                Key = $"{++idx}",
                Text = ToLabelText($"{nameof(ImportFlowerWarehouseData).ToCamelCaseSplit()}", "Import data from CSV file"),
                Action = (self) =>
                {
#if DEBUG && DEVELOP_ON
                    PrintHeader();
                    var success = ImportFlowerWarehouseData();
                    
                    if (success)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        PrintLine("===================================================");
                        PrintLine("SUCCESS - CSV Import completed!");
                        PrintLine("===================================================");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        PrintLine("===================================================");
                        PrintLine("FAILED - CSV Import failed!");
                        PrintLine("===================================================");
                        Console.ResetColor();
                    }
                    
                    PrintLine();
                    PrintLine("Press any key to continue...");
                    Console.ReadKey();
#endif
                },
#if DEBUG && DEVELOP_ON
                ForegroundColor = ConsoleApplication.ForegroundColor,
#else
                ForegroundColor = ConsoleColor.Red,
#endif
            });
            menuIdx = idx;
        }
        #endregion partial method implementations

        #region import methods
        /// <summary>
        /// Imports flower warehouse data from CSV file.
        /// Returns true if successful, false otherwise.
        /// </summary>
        private bool ImportFlowerWarehouseData()
        {
            try
            {
                if (!File.Exists(CsvFilePath))
                {
                    PrintLine($"ERROR: CSV file not found at: {CsvFilePath}");
                    return false;
                }

                var lines = File.ReadAllLines(CsvFilePath);
                
                if (lines.Length < 2)
                {
                    PrintLine("ERROR: CSV file is empty or contains only header.");
                    return false;
                }

                using var context = CreateContext();
                var dataLines = lines.Skip(1).ToArray();

                PrintLine($"Processing {dataLines.Length} records...");
                PrintLine();

                var categoryDict = new Dictionary<string, IdType>();
                var supplierDict = new Dictionary<string, IdType>();
                
                int categoriesCreated = 0, suppliersCreated = 0, articlesCreated = 0, articlesSkipped = 0;

                foreach (var line in dataLines)
                {
                    try
                    {
                        var columns = line.Split(';');
                        if (columns.Length != 8) { articlesSkipped++; continue; }

                        var articleNumber = columns[0].Trim();
                        var productName = columns[1].Trim();
                        var categoryName = columns[2].Trim();
                        var purchasePriceStr = columns[3].Trim();
                        var salesPriceStr = columns[4].Trim();
                        var stockStr = columns[5].Trim();
                        var supplierName = columns[6].Trim();
                        var expiryDateStr = columns[7].Trim();

                        // Category
                        if (!categoryDict.ContainsKey(categoryName))
                        {
                            var task = Task.Run(async () => await context.CategorySet.GetAsync());
                            var categories = task.Result;
                            var existingCategory = categories.FirstOrDefault(c => c.Name == categoryName);

                            if (existingCategory != null)
                            {
                                categoryDict[categoryName] = existingCategory.Id;
                            }
                            else
                            {
                                var newCategory = new Category
                                {
                                    Name = categoryName,
                                    Description = categoryName == "Topfpflanze" ? "Potted plants" : "Fresh cut flowers"
                                };
                                Task.Run(async () => await context.CategorySet.AddAsync(newCategory)).Wait();
                                Task.Run(async () => await context.SaveChangesAsync()).Wait();
                                categoryDict[categoryName] = newCategory.Id;
                                categoriesCreated++;
                            }
                        }

                        // Supplier
                        if (!supplierDict.ContainsKey(supplierName))
                        {
                            var task = Task.Run(async () => await context.SupplierSet.GetAsync());
                            var suppliers = task.Result;
                            var existingSupplier = suppliers.FirstOrDefault(s => s.Name == supplierName);

                            if (existingSupplier != null)
                            {
                                supplierDict[supplierName] = existingSupplier.Id;
                            }
                            else
                            {
                                var newSupplier = new Supplier { Name = supplierName };
                                Task.Run(async () => await context.SupplierSet.AddAsync(newSupplier)).Wait();
                                Task.Run(async () => await context.SaveChangesAsync()).Wait();
                                supplierDict[supplierName] = newSupplier.Id;
                                suppliersCreated++;
                            }
                        }

                        // Check Article exists
                        var articlesTask = Task.Run(async () => await context.ArticleSet.GetAsync());
                        if (articlesTask.Result.Any(a => a.ArticleNumber == articleNumber))
                        {
                            articlesSkipped++;
                            continue;
                        }

                        // Parse values
                        if (!decimal.TryParse(purchasePriceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var purchasePrice) ||
                            !decimal.TryParse(salesPriceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var salesPrice) ||
                            !int.TryParse(stockStr, out var stock) ||
                            !DateTime.TryParse(expiryDateStr, out var expiryDate))
                        {
                            articlesSkipped++;
                            continue;
                        }

                        // Create Article
                        var article = new Article
                        {
                            ArticleNumber = articleNumber,
                            ProductName = productName,
                            PurchasePrice = purchasePrice,
                            SalesPrice = salesPrice,
                            Stock = stock,
                            ExpiryDate = expiryDate,
                            CategoryId = categoryDict[categoryName],
                            SupplierId = supplierDict[supplierName]
                        };

                        Task.Run(async () => await context.ArticleSet.AddAsync(article)).Wait();
                        articlesCreated++;
                    }
                    catch (Exception lineEx)
                    {
                        PrintLine($"ERROR: {lineEx.Message}");
                        articlesSkipped++;
                    }
                }

                Task.Run(async () => await context.SaveChangesAsync()).Wait();

                PrintLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                PrintLine($"Categories: {categoriesCreated}, Suppliers: {suppliersCreated}, Articles: {articlesCreated}");
                Console.ResetColor();
                PrintLine($"Skipped: {articlesSkipped}");
                
                return true;
            }
            catch (Exception ex)
            {
                PrintLine($"ERROR: {ex.Message}");
                return false;
            }
        }
        #endregion import methods
    }
}


 ```

 2. **Validierungs-Klassen erstellen:**
   - Datei: `ProjectName.Logic/Entities/{Data|App|Account}/EntityName.Validation.cs`
   - `IValidatableEntity` implementieren
   - Validierungsregeln aus Beschreibung umsetzen



### 5. Anleitung für frontend erstellen

- Erstellung einer Beschreibung des Projektes, der Funktionalitäten und der endpoints als Markdown file.

### 6. Änderungen und Erweiterungen

- Änderungen die die Entitäten betreffen
  - Zuerst die generierten Klassen entfernen:
    1. Delete generierte Klassen: 
    `dotnet run --project TemplateTools.ConApp -- AppArg=4,7,x,x`
- Dannach starte wieder beim Workflow bei Punkt 1.



## Troubleshooting

### Häufige Probleme

- **Build-Fehler**: Code-Generierung ausführen nach Entity-Änderungen
- **Import-Fehler**: CSV-Format und Beziehungen prüfen
- **Routing**: Komponenten in `app-routing.module.ts` registrieren

### Debugging

- Generated Code über `//@AiCode` Marker identifizieren
- Custom Code in separaten Bereichen isolieren
- Console-App für Datenbank-Tests nutzen


### Phase 7: Testen

#### Checkliste:

-  Backend: Datenbank ist angelegt und enthält die benötigten Entitäten.
- Backend: API-Endpoints funktionieren (Swagger UI)
-  Backend: Datenbank ist angelegt und enthält die benötigten Entitäten

### Bei Änderungen

Bevor Änderungen am Projekt durchgeführt werden können, müssen generierte Klassen mit TemplateTools.ConApp gelöscht und anschließend wieder neu generiert werden.