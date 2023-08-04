# SQL Migrator

> Work In Progress

When designing the next generation of an application, the evolved schema of the new application does not always easily conform to the schema of the original application. This is particularly true when:

* The standards and policies that drive the application evolve beyond its original scope and purpose
* The technologies that support the initial application greatly atrophy

The project within this repository uses a small schema derived from the [AdventureWorks](https://learn.microsoft.com/en-us/sql/samples/adventureworks-install-configure) database to demonstrate how to simplify data migrations through a command-line interface. It provides a clean way of translating the data in the initial app schema to the updated app schema. It is based on a real world scenario that leveraged this approach.