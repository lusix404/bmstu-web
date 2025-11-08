--зарегестрированный пользователь
CREATE ROLE ordinary_user WITH
LOGIN
PASSWORD 'user_web_2025';

CREATE ROLE moderator WITH
LOGIN
PASSWORD 'moder_web_2025';

CREATE ROLE administrator WITH
LOGIN
PASSWORD 'admin_web_2025';


--пользователь
grant select, insert, delete, update on users to ordinary_user;
grant select on drinks to ordinary_user ;
grant select on companies to ordinary_user ;
grant select on coffeeshops to ordinary_user ;
grant select on categories to ordinary_user ;
grant select on drinkscategory to ordinary_user ;
grant select on menu to ordinary_user ;
grant select, insert, delete on favdrinks to ordinary_user ;
grant select,insert, delete on favcoffeeshops to ordinary_user ;
grant select on roles to ordinary_user ;


--модератор


grant select, insert, delete, update on users to moderator;
grant select,insert, delete on drinks to moderator ;
grant select, update,insert, delete  on companies to moderator;
grant select,insert, delete on coffeeshops to moderator;
grant select, insert on categories to moderator;
grant select, insert, delete, update on drinkscategory to moderator;
grant select,insert, delete on menu to moderator;
grant select, insert, delete on favdrinks to moderator;
grant select,insert, delete on favcoffeeshops to moderator;
grant select on roles to moderator;
   
--администратор
grant select,insert, delete on drinks to administrator;
grant select, update, insert, delete on companies to administrator;
grant select,insert, delete on coffeeshops to administrator;
grant select, insert on categories to administrator;
grant select, insert, delete, update on drinkscategory to administrator;
grant select,insert, delete on menu to administrator;
grant select, insert, delete on favdrinks to administrator;
grant select,insert, delete on favcoffeeshops to administrator;
grant select on roles to administrator;
grant select, insert, delete, update on users to administrator;