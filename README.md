Hello!
Here is Admin panel project
In ./AdminPanelBackend here is backend part
In ./AdminPanelFrontend here is frontent.
## Launching
To start the project all you need is to run command 
```d
docker-compose up --build
```

All migrations and needed information will be created.
Also by default on launching will be created 2 users

```
Super Admin
email: admin@example.com
pasword: Admin123!
```

And another one
```
Game manager
email: examplemanager@example.com
password: pass123
```

## Role system

I decided create role system based on permissions to concrete resources
For instance we have GameManagement module and it could have this list of permissions
```
gm.games.read
gm.games.edit
gm.games.feature.set
gm.games.visibility.update
...
```
And this gives us flexibility for creating different custom roles based on our needs

## Backend application layers
1. AdminPanel - presentation layer with controllers, API response models and middlewares
2. AdminPanel.Domain - domain layer where entities, interfaces for services, request models, and roles stored
3. AdminPanel.Application - application layer here realisation of services located.
   Also I want to point out that I decided not to use repositories in this project due to time saving reasons and to have more time to concentrate on roles system so I created IAppDbContext in Application layer to keep dependencies clear.
4. AdminPanel.Infrastructure - infrastructure level here we have migrations, some helpers, seeder for database etc

