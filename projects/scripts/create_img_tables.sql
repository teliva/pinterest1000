-- Create Image Database

IF DB_ID(N'ImageDatabase') IS NULL
BEGIN
    CREATE DATABASE ImageDatabase;
END
GO

USE ImageDatabase;
GO

-- Create tables for image categorization in Microsoft SQL Server
IF OBJECT_ID('dbo.image_categories', 'U') IS NULL
BEGIN
    CREATE TABLE image_categories (
        category_id INT IDENTITY(1,1) PRIMARY KEY,
        description VARCHAR(50) NOT NULL UNIQUE
    );
END

IF OBJECT_ID('dbo.image_room_type', 'U') IS NULL
BEGIN
    CREATE TABLE image_room_type (
        room_type_id INT IDENTITY(1,1) PRIMARY KEY,
        description VARCHAR(50) NOT NULL UNIQUE
    );
END

IF OBJECT_ID('dbo.image_style', 'U') IS NULL
BEGIN
    CREATE TABLE image_style (
        style_id INT IDENTITY(1,1) PRIMARY KEY,
        description VARCHAR(50) NOT NULL UNIQUE
    );
END

IF OBJECT_ID('dbo.image', 'U') IS NULL
BEGIN
    CREATE TABLE image (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        category_id INT NOT NULL,
        room_type_id INT NOT NULL,
        created_at DATETIME2 DEFAULT GETDATE(),
        FOREIGN KEY (category_id) REFERENCES image_categories(category_id) ON DELETE NO ACTION,
        FOREIGN KEY (room_type_id) REFERENCES image_room_type(room_type_id) ON DELETE NO ACTION
    );
END

IF OBJECT_ID('dbo.image_image_style', 'U') IS NULL
BEGIN
    CREATE TABLE image_image_style (
        image_id UNIQUEIDENTIFIER NOT NULL,
        style_id INT NOT NULL,
        PRIMARY KEY (image_id, style_id),
        FOREIGN KEY (image_id) REFERENCES image(id) ON DELETE NO ACTION,
        FOREIGN KEY (style_id) REFERENCES image_style(style_id) ON DELETE NO ACTION
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.image') AND name = 'idx_image_category_id')
BEGIN
    CREATE INDEX idx_image_category_id ON image(category_id);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.image') AND name = 'idx_image_room_type_id')
BEGIN
    CREATE INDEX idx_image_room_type_id ON image(room_type_id);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.image_image_style') AND name = 'idx_image_image_style_style_id')
BEGIN
    CREATE INDEX idx_image_image_style_style_id ON image_image_style(style_id);
END
