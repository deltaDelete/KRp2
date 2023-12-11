create table Consultations
(
    Id    int auto_increment
        primary key,
    Title longtext       not null,
    Price decimal(30, 2) not null,
    Date  datetime(6)    not null,
    Time  time(6)        not null
);

create table Owners
(
    Id        int auto_increment
        primary key,
    FullName  longtext    not null,
    Passport  bigint      not null,
    BirthDate datetime(6) not null
);

create table Appointments
(
    Id             int auto_increment
        primary key,
    OwnerId        int not null,
    ConsultationId int not null,
    constraint foreign key (ConsultationId) references Consultations (Id)
            on delete cascade,
    constraint foreign key (OwnerId) references Owners (Id)
            on delete cascade
);
