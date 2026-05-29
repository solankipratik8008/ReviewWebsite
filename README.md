



# CoffeeShot

CoffeeShot is an ASP.NET MVC review web application for coffee shops, coffee products, and coffee-related food items. It allows users to register, log in, add reviews, upload images, give star ratings, filter reviews, and contact the admin.

The goal of this project is to help customers share honest feedback and help coffee shop owners understand customer likes, dislikes, strengths, and improvement areas.

## Project Demo

[![CoffeeShot Project Demo](https://img.youtube.com/vi/NuI8AOkovwA/maxresdefault.jpg)](https://youtu.be/NuI8AOkovwA)

**Watch Demo:** https://youtu.be/NuI8AOkovwA

## Tech Stack

* **Frontend:** HTML, CSS
* **Backend:** ASP.NET, C#
* **Architecture:** MVC
* **Database:** SQLite / SQL Server option
* **Framework:** .NET 9

## Features

* User registration and login
* Role-based access for users and admin
* Add and edit reviews
* Image upload for reviews
* Star rating system
* Search and filter reviews
* Rating slider and date range filter
* Contact/message form for admin
* Admin panel for review management
* Coin reward feature: users earn 5 coins per review

## Run Locally

Install the .NET 9 SDK first.

From the repository root, run:

```bash
dotnet restore ReviewSite.sln
dotnet build ReviewSite.sln
dotnet run --project ReviewSite/ReviewSite.csproj
```

Open the URL shown in the terminal, usually:

```text
http://localhost:5210
```

## Seeded Login Accounts

**Admin**

```text
Username: ConestogaCollege
Password: 123
```

**User**

```text
Username: user1
Password: password
```

## My Contributions

* Developed the coin reward feature, giving users 5 coins per review
* Implemented user registration and login
* Displayed logged-in username and admin status
* Created the star rating display logic
* Added search, rating slider, and date range filters
* Worked on image save, display, and image flipping preview
* Enabled users to send messages to the admin
* Managed temporary database storage during development

## Contributors

**Pratik Kumar Solanki**
Authentication, coin reward feature, filters, star rating logic, image handling, admin messaging, and database support.

**Sharan**
Review CRUD operations, data seeding, and website design.

**Anmol**
Website content, images, and design support.

## Lessons Learned

This project improved my understanding of ASP.NET MVC, authentication, admin panel development, image handling, database integration, filtering logic, and team-based web application development.

## Contributing

Pratik Kumar Solanki : 
1. Developed coin feature : allocating 5 coins per review to the user.
2. Image save & Display : designed an image viewer which flips dual images on the screen to show both.
3. Review Stars : Created star filling function, which fills the number of stars as per the rating provided by the user.
4. Filters : Executed multiple filter application including slider for number of ratings, search functionality, datepicker etc.
5. Login : Implemented user registration and login and displaying user name while the user is logged in. On the other hand displaying admin when the login takes place as admin role and with specified user name and password allocated to the admin.
6. Managed Database for temporary data saving.
7. Message : enabled messaging admin by all the users through a form provided.

Sharan : 
1. CRUD Operations : Handled Creating, editing, updating and deleting reviews.
2. Data Seeding : worked on seeding reviews.
3. Designing : worked on designing such as color scheme etc.

Anmol : 
1. Content Creater : Provided all the content including images for the website.
2. Designing : participated in designing for the website.

