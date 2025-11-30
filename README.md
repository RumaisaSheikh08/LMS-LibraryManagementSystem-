
LMS Library Management System

A web-based Library Management System built with ASP.NET / C# that helps manage books, users, borrowing/return transactions — making library operations simpler and automated.  
This project also integrates **SSRS Reporting** and **ETL (Extract–Transform–Load)** workflows for advanced reporting and automated data synchronization.

Overview
The LMS provides a complete digital library experience, replacing manual workflows with automated book management, member management, and reporting modules.  
It includes:  
- A web-based interface (ASP.NET MVC)  
- Reporting through **SQL Server Reporting Services (SSRS)**  
- Background processes and Data Integration using **ETL** packages  

Features
- **Book Management:** Add, update, delete and track book details.  
- **User / Member Management:** Manage member profiles and borrowing privileges.  
- **Borrow / Return System:** Automated availability updates.  
- **Catalog Search:** Search and filter books by title, author, category, or status.  
- **SSRS Reporting Module:**  
  - Book Inventory Reports  
  - Borrow/Return Audit  
  - Overdue Books Report  
  - Member Activity  
- **ETL Integration:**  
  - Automated data loading from external sources  
  - Cleaning and transformation workflows  
  - Scheduled data refresh  

Tech Stack
- **Framework:** ASP.NET / C# (MVC Architecture)  
- **Database:** SQL Server  
- **Reporting:** SSRS (SQL Server Reporting Services)  
- **ETL Tool:** SSIS / ETL scripts  
- **Frontend:** Razor Views, HTML, CSS, JS  
- **Configuration:** appsettings.json  

Project Structure
- **Controllers/** – Request handling logic  
- **Models/** – Entity classes  
- **Data/** – Database context and integration logic  
- **LMSDateIntegration/** – ETL/Integration modules  
- **LMSReportProject/** – SSRS Report definitions and report viewer integration  
- **Views/** – UI pages  
- **wwwroot/** – Static assets  
- **appsettings.json** – App configuration  
- **.sln / .csproj** – Project solution files  

SSRS Reporting Overview
The system includes a complete reporting module using **SQL Server Reporting Services (SSRS)**:  
- Reports are built using `.rdl` files inside **LMSReportProject**  
- Integrated with the website through ReportViewer / embedded endpoints  
- Allows exporting to PDF, Excel, CSV  
- Helps administrators monitor performance and library activity  

ETL Workflow (Data Integration)
The **ETL module** under *LMSDateIntegration* handles:  
- Importing book/member data from external data sources  
- Running scheduled SSIS/ETL workflows  
- Data cleaning, transformation, and validation  
- Pushing cleaned data into the main LMS database  

Used for:  
- Migrating old library data  
- Quickly adding large datasets  
- Keeping reports refreshed for SSRS  

Setup & Installation
1. Clone repo  
2. Open solution in Visual Studio  
3. Configure database in `appsettings.json`  
4. Restore NuGet packages  
5. Configure SSRS URL in reports module (if applicable)  
6. Run ETL scripts / SSIS workflow if required  
7. Build and run  

Usage
- Manage books, members  
- Perform borrow and return operations  
- View SSRS reports  
- Trigger or monitor ETL data sync  

Contributing
Fork → Branch → Commit → PR

