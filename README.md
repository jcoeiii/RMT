# RMT

## C# Example for SQLite which is similar to MS Access database
## PDF File creation programmatically from C# using PDFsharp
## Better and cleaner abstraction from GUI to dbase (Sql.cs)




PDFsharp is the Open Source .NET library that easily creates and processes PDF documents on the fly from any .NET language. The same drawing routines can be used to create PDF documents, draw on the screen, or send output to any printer.

This is PDFsharp based on GDI+. See Project Information for details.
A .NET library for processing PDF. This is PDFsharp based on GDI+. See Project Information for details.

http://www.nuget.org/packages/PDFsharp

Important nuts & bolts of this project:

Print2Pdf.cs
============
This object creates/draws the PDF document based on PDF Sharp.  It is a convenience wrapper
to draw text, tables, and lines more easily and helps to keep track of your current text
locations.  See FormMainPrinting.cs on how to use this object effectively.

Orders.cs & Quotes.cs
=====================
These classes provide the abstraction from UI to database as the Main Form only contains
two distinct tabs: Orders and Quotes.  These two important classes contain the Form’s
control names, the default value for a new record, and the corresponding table column name
inside the database.  These objects also contain the Sql command text generators for ease
of use; e.g., the update commands are generated here based on the table column names as a
quick and easy convenience.

Sql.cs
======
This lower-level static object manages and maintains the Sql connection, and provides static
functions to support reading table row counts, adding/deleting rows, and
reading/updating/inserting rows, to and from the Sql database in one place.  Note that some
of the sub-forms have not been updated to use this object 100% yet.

