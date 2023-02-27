use alhambra;
-- insert into user(id, username, mail) values(4, 'jakub', 'jakub452@gmail.com');
-- select * from user;
/*
CREATE TABLE neco (
attribute1 int(4) AUTO_INCREMENT,
attribute2 varchar(45) DEFAULT NULL,
PRIMARY KEY (attribute1)
);
*/
/*
insert into neco(attribute2) values('cosi');
select * from neco;
*/
/*
alter table user
modify column id int AUTO_INCREMENT;
*/
update user
set lastLogin = now()
where id = 4;



