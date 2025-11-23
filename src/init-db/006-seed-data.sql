-- Seed application data from CSV files
-- The files are mounted to /docker-entrypoint-initdb.d in the containers

-- Users
COPY users(id_user, id_role, login, password, birthdate, email)
FROM '/docker-entrypoint-initdb.d/users.csv'
DELIMITER ','
CSV HEADER;

-- Categories
COPY categories(id_category, name)
FROM '/docker-entrypoint-initdb.d/categories.csv'
DELIMITER ','
CSV HEADER;

-- Drinks
COPY drinks(id_drink, name)
FROM '/docker-entrypoint-initdb.d/drinks.csv'
DELIMITER ','
CSV HEADER;

-- Companies
COPY companies(id_company, name, website)
FROM '/docker-entrypoint-initdb.d/companies.csv'
DELIMITER ','
CSV HEADER;

-- Coffee shops
COPY coffeeshops(id_coffeeshop, id_company, address, workinghours)
FROM '/docker-entrypoint-initdb.d/coffeeshops.csv'
DELIMITER ','
CSV HEADER;

-- Menu
COPY menu(id_menu, id_drink, id_company, size, price)
FROM '/docker-entrypoint-initdb.d/menu.csv'
DELIMITER ','
CSV HEADER;

-- Drink categories
COPY drinkscategory(id_drink, id_category)
FROM '/docker-entrypoint-initdb.d/drinkscategory.csv'
DELIMITER ','
CSV HEADER;

-- Favorites
COPY favdrinks(id_user, id_drink)
FROM '/docker-entrypoint-initdb.d/favdrinks.csv'
DELIMITER ','
CSV HEADER;

COPY favcoffeeshops(id_user, id_coffeeshop)
FROM '/docker-entrypoint-initdb.d/favcoffeeshops.csv'
DELIMITER ','
CSV HEADER;
