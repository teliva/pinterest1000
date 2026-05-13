USE ImageDatabase;
GO

INSERT INTO [dbo.image_categories] (description)
VALUES
    ('Meeting & Collaboration'),
    ('Social Culture & Support'),
    ('Primary Workspaces'),
    ('Healthcare');
GO

INSERT INTO [dbo.image_room_type] (description)
VALUES
    ('Breakout Area / Open Meeting'),
    ('Conference / Boardroom'),
    ('Meeting Room (Small) / Huddle'),
    ('Brainstorm / Project Room'),
    ('Pantry / Café / Kitchen'),
    ('Reception Area / Waiting'),
    ('Work Lounge / Staff Lounge'),
    ('Wellness & Recreation'),
    ('Library / Quiet Zone'),
    ('Open-Plan Office / Benching'),
    ('Private Office / Exec Suite'),
    ('Touch Down / Hot Desk'),
    ('Focus Room / Phone Booth'),
    ('Home / Garden Office'),
    ('Open Space');
GO

INSERT INTO [dbo.image_style] (description)
VALUES
    ('Minimalist'),
    ('Mid-Century'),
    ('Modern'),
    ('Industrial'),
    ('Luxury/Exec'),
    ('Biophilic');
GO
