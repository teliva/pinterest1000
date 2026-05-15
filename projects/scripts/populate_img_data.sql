USE [ImageDatabase];
GO

INSERT INTO [dbo].[image_categories] (description)
VALUES
    ('Meeting & Collaboration'),
    ('Social Culture & Support'),
    ('Primary Workspaces'),
    ('Healthcare');
GO

INSERT INTO [dbo].[image_room_type] (description)
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

INSERT INTO [dbo].[image_style] (description)
VALUES
    ('Minimalist'),
    ('Mid-Century'),
    ('Modern'),
    ('Industrial'),
    ('Luxury/Exec'),
    ('Biophilic');
GO

INSERT INTO [dbo].[image] (id, category_id, room_type_id)
VALUES
    ('15abacb7-475b-48c3-a81e-78d66a9e1a30', 3, 11),
    ('1bb3eb27-85f9-4f63-9dc6-9343aa8ac96a', 3, 11),
    ('3cb3d32d-f296-4eeb-944e-97c6bd7a2a19', 3, 11),
    ('44acb21b-db2d-480f-a4b0-de4440d20c9e', 3, 10),
    ('8c408ee8-ff22-4250-9e6c-5afe591c80ed', 1, 2),
    ('af3b0581-938b-4f63-b3ae-f92ed099f289', 1, 11),
    ('da12549a-5942-4352-9917-9b3ac7b2396f', 3, 2),
    ('de7c7076-697c-49c4-9db2-9fa398b73de9', 1, 11);
GO

INSERT INTO [dbo].[image_image_style] (image_id, style_id)
VALUES
    ('15abacb7-475b-48c3-a81e-78d66a9e1a30', 4),
    ('1bb3eb27-85f9-4f63-9dc6-9343aa8ac96a', 3),
    ('3cb3d32d-f296-4eeb-944e-97c6bd7a2a19', 3),
    ('44acb21b-db2d-480f-a4b0-de4440d20c9e', 3),
    ('8c408ee8-ff22-4250-9e6c-5afe591c80ed', 4),
    ('af3b0581-938b-4f63-b3ae-f92ed099f289', 3),
    ('da12549a-5942-4352-9917-9b3ac7b2396f', 4),
    ('de7c7076-697c-49c4-9db2-9fa398b73de9', 4);
GO

