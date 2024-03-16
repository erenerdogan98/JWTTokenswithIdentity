I tried to simply explain Authantication (Identity) and JWT, which I use frequently in my projects.I tried to make it as simple as possible and following Solid.

I would like to briefly explain the operations carried out on the project.
For simplicity, we created a single-layer (Web API) project.
Entity Framework packages installed (including Identity and JwtBearer).
MySql was used for the database, first the connection string was added to appsettings.json.
Model folder was created and 2 more folders were created (DTO and Domain)
For Identity User, a User class (AppUser) was created and inherited from IdentityUser. (In my more comprehensive projects, I added different properties, different props can be defined, I defined 'Name' for simple demonstration.)
For Identity Role, the AppRole class has been added into the Models folder.
We created a class named Context in the Domain folder and made the database connection (by inheriting from IdentityDbContext and used AppUser class).
A class was created in the Domain folder product for token values (TokenInfo).
Classes were created in the DTO for the operations to be performed and properties were simply added.
To use the Repository Design pattern, a folder was created and 2 folders were added (Abstract and Domain).
Token services were created (ITokenService in the interface Abstract, and TokenService class in the Domain), the interface was implemented and the relevant methods were added.
Necessary configurations and service additions for dependency injection were made in Program.cs.
First, a controller was added for Token transactions and 2 methods (Refresh and Revoke, revoke with Authorize) were defined.
Authorization controller was created for login and registration operations with Identity (using Token).(with Login, Register and Change Password method)
All transactions were tested through the postman agent and were carried out successfully.
Finally, 2 controllers named Admin and Protected were added for testing and tested successfully.

I wanted to share the project in which I demonstrate the Authorization and Authantication processes in a simple way, using Identity and Json Web Token. I tried to explain this method, which is used in almost every project, in a simple way.



