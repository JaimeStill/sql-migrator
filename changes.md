# Changes

* Refactor `IMigrationTarget.SourceId` to `OriginKey`

* Added the following entity models to demonstrate seeding:

    * `Company` demonstrates a foreign key seeding sceanrio. One `Company` is associatd with many `Department` records. When migrated `Department`, a `CompanySeeder` will be used to set `CompanyId` based on `CompanySeeder.Default`.

    * `Message` demonstrates a nav collection seeding scenario. One `Employee` can have many `Messages`. When migrating an `Employee` record, initialize the `Employee.Messages` collection before saving the `Employee`. Still need to flesh out the strategy for this vs. foreign key seeding.

* Created EF `MessageConfig`, added models to `AppDbContext` and created a new EF migration.

* Created migration models for `Company` and `Message` in **Core/Schema/AdventureWorks**.

* Move `Translator` into a **Sequence** directory. All classes that move data into the target database will derive from a `Sequence` class to standardize common data sequence functionality. `Translator` will be used when translating data from an origin database into the target database. `Seeder` will be used when injecting new baseline data into the target database.

* Move all `*Config` records in **Core/Sql** into a **Core/Sql/Config** directory.

* Establish `SequenceConfig` to serve as the base for all `Sequence` classes. Moved everything from `TranslatorConfig` to `SequenceConfig` minus `Connector Source`. Derive `TranslatorConfig` from `SequenceConfig` which defines just `Connector Source`.

* Created `AwSeeder` and `CompanySeeder` at **Cli/Seeders**. `AwSeeder` serves as a base class for all seeders that migrate data based on the `IMigrationTarget` signature. `CompanySeeder` derives from `AwSeeder`.

* In `EmployeeTranslator`, added an override for `OnInsert` that initializes a default `Message` within `Employee.Inbox`.

* In `DepartmentTranslator`, initialize a readonly `CompanySeeder` property in the constructor. Added an override for `OnInsert` that sets `department.CompanyId` to the value of `companySeeder.EnsureMigrated(companySeeder.Default)`.