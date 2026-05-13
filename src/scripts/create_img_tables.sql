-- Create tables for image categorization in Microsoft SQL Server
CREATE TABLE image_categories (
    category_id INT IDENTITY(1,1) PRIMARY KEY,
    description VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE image_room_type (
    room_type_id INT IDENTITY(1,1) PRIMARY KEY,
    description VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE image_style (
    style_id INT IDENTITY(1,1) PRIMARY KEY,
    description VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE image (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    category_id INT NOT NULL,
    room_type_id INT NOT NULL,
    style_id INT NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (category_id) REFERENCES image_categories(category_id) ON DELETE NO ACTION,
    FOREIGN KEY (room_type_id) REFERENCES image_room_type(room_type_id) ON DELETE NO ACTION,
    FOREIGN KEY (style_id) REFERENCES image_style(style_id) ON DELETE NO ACTION
);

-- Indexes for performance on foreign keys
CREATE INDEX idx_image_category_id ON image(category_id);
CREATE INDEX idx_image_room_type_id ON image(room_type_id);
CREATE INDEX idx_image_style_id ON image(style_id);