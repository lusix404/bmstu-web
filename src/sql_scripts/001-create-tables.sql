----ТАБЛИЦА ROLES-------
create table if not exists roles (
	id_role int primary key,
	name varchar(128) not null, 
	check (name = 'user' or name = 'moderator' or name = 'administrator')
);

---ТАБЛИЦА USERS-------
create table if not exists users (
	id_user UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	id_role int not null,
	login varchar(128) not null unique,
	password varchar(128) not null,
	birthdate date not null,
	check (birthdate < CURRENT_DATE),
	email varchar(256) not null
);
alter table users
    add foreign key(id_role) references roles(id_role);


---ТАБЛИЦА DRINKS-----
CREATE TABLE IF NOT EXISTS drinks(
    id_drink UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(128) NOT NULL UNIQUE
);
 
---ТАБЛИЦА CATEGORIES-----
create table if not exists categories(
	id_category UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	name varchar(128) NOT NULL UNIQUE
);


---ТАБЛИЦА DRINKSCATEGORY------
create table if not exists drinkscategory(
	id_drink uuid not null,
	id_category uuid not null,
	primary key (id_drink, id_category)
);
alter table drinkscategory
    add foreign key(id_drink) references drinks(id_drink);
alter table drinkscategory
    add foreign key(id_category) references categories(id_category);
  
   
------ТАБЛИЦА FAVDRINKS-------
create table if not exists favdrinks(
	id_user uuid not null,
	id_drink uuid not null,
	primary key (id_user, id_drink)
);
alter table favdrinks
    add foreign key(id_drink) references drinks(id_drink);
alter table favdrinks
    add foreign key(id_user) references users(id_user);
    

------ТАБЛИЦА COMPANIES------
create table if not exists companies(
	id_company UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	name varchar(128) not null,
	website varchar (256)
);

------ТАБЛИЦА MENU---------
create table if not exists menu(
	id_drink uuid not null,
	id_company uuid not null,
    primary key (id_drink, id_company)
	size int not null check (size > 0),
	price numeric(10,2) not null CHECK (price >= 0)
);
alter table menu
    add foreign key(id_drink) references drinks(id_drink);
alter table menu
    add foreign key(id_company) references companies(id_company);

   
-------ТАБЛИЦА COFFEESHOPS--------
create table if not exists coffeeshops(
	id_coffeeshop UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	id_company uuid not null,
	address varchar(256) not null,
	workinghours varchar(64) not null
);
alter table coffeeshops
    add foreign key(id_company) references companies(id_company);

-----ТАБЛИЦА FAVCOFFEESHOPS---------
create table if not exists favcoffeeshops(
	id_user uuid not null,
	id_coffeeshop uuid not null,
	primary key (id_user, id_coffeeshop)
);

alter table favcoffeeshops
    add foreign key(id_coffeeshop) references coffeeshops(id_coffeeshop);
alter table favcoffeeshops
    add foreign key(id_user) references users(id_user);
   