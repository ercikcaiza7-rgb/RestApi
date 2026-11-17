USE RestauranteITQ;
CREATE TABLE Usuarios (
  Id           INT IDENTITY(1,1) PRIMARY KEY,
  Nombre       NVARCHAR(100) NOT NULL,
  Correo       NVARCHAR(150) NOT NULL UNIQUE,
  PasswordHash NVARCHAR(100) NOT NULL,
  Rol          NVARCHAR(20)  NOT NULL
    CHECK (Rol IN ('Administrador','Mesero','Cocina')),
  Activo       BIT NOT NULL DEFAULT 1
);

select*from Usuarios;