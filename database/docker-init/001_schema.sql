create table if not exists schema_migrations (
    id text primary key,
    applied_at timestamp with time zone not null
);

create table if not exists parking_lots (
    id uuid primary key,
    created_at timestamp with time zone not null
);

create table if not exists parking_spots (
    id uuid primary key,
    parking_lot_id uuid not null references parking_lots(id) on delete cascade,
    spot_number integer not null,
    size text not null check (size in ('Small', 'Regular', 'Large')),
    unique (parking_lot_id, spot_number)
);

create table if not exists parked_vehicles (
    id uuid primary key,
    parking_lot_id uuid not null references parking_lots(id) on delete cascade,
    license_plate text not null,
    vehicle_type text not null check (vehicle_type in ('Motorcycle', 'Car', 'Van')),
    parked_at timestamp with time zone not null,
    unique (parking_lot_id, license_plate)
);

create table if not exists vehicle_spots (
    vehicle_id uuid not null references parked_vehicles(id) on delete cascade,
    spot_id uuid not null references parking_spots(id) on delete cascade,
    primary key (vehicle_id, spot_id),
    unique (spot_id)
);

create index if not exists ix_parking_spots_parking_lot_id
    on parking_spots (parking_lot_id);

create index if not exists ix_parked_vehicles_parking_lot_id
    on parked_vehicles (parking_lot_id);

insert into schema_migrations (id, applied_at)
values ('001_create_parking_lot_tables', now())
on conflict (id) do nothing;
