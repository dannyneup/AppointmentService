create table therapist
(
    id   integer      not null primary key,
    name varchar(255) not null
);

create table individual_remedy
(
    id   integer      not null primary key,
    name varchar(255) not null
);

create table appointment
(
    id                          integer      not null primary key,
    start                       timestamp    not null,
    "end"                       timestamp    not null,
    patient_insurance_number    varchar(255) not null,
    therapist_id                integer      not null
        constraint fk_app_therapist references therapist (id),
    practice_institution_code   varchar(9)   not null,
    fixed_remedy_diagnosis_code varchar(255),
    individual_remedy_id        integer references individual_remedy (id),
    constraint chk_exactly_one_remedy check (
        (individual_remedy_id is not null) <> (fixed_remedy_diagnosis_code is not null)
        )
);
