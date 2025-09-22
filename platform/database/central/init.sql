create table practice
(
    institution_code varchar(9)   not null primary key,
    name             varchar(255) not null
);

create table patient
(
    insurance_number varchar(255) not null primary key,
    name             varchar(255) not null,
    age              integer
);

create table fixed_remedy
(
    diagnosis_code varchar(255) not null primary key,
    name           varchar(255) not null
);