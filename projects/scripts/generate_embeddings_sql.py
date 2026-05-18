import urllib.request
import json
import os

categories = [
    'Meeting & Collaboration',
    'Social Culture & Support',
    'Primary Workspaces',
    'Healthcare'
]

room_types = [
    'Breakout Area / Open Meeting',
    'Conference / Boardroom',
    'Meeting Room (Small) / Huddle',
    'Brainstorm / Project Room',
    'Pantry / Café / Kitchen',
    'Reception Area / Waiting',
    'Work Lounge / Staff Lounge',
    'Wellness & Recreation',
    'Library / Quiet Zone',
    'Open-Plan Office / Benching',
    'Private Office / Exec Suite',
    'Touch Down / Hot Desk',
    'Focus Room / Phone Booth',
    'Home / Garden Office',
    'Open Space'
]

styles = [
    'Minimalist',
    'Mid-Century',
    'Modern',
    'Industrial',
    'Luxury/Exec',
    'Biophilic'
]

images = [
    ('15abacb7-475b-48c3-a81e-78d66a9e1a30', 3, 11),
    ('1bb3eb27-85f9-4f63-9dc6-9343aa8ac96a', 3, 11),
    ('3cb3d32d-f296-4eeb-944e-97c6bd7a2a19', 3, 11),
    ('44acb21b-db2d-480f-a4b0-de4440d20c9e', 3, 10),
    ('8c408ee8-ff22-4250-9e6c-5afe591c80ed', 1, 2),
    ('af3b0581-938b-4f63-b3ae-f92ed099f289', 1, 11),
    ('da12549a-5942-4352-9917-9b3ac7b2396f', 3, 2),
    ('de7c7076-697c-49c4-9db2-9fa398b73de9', 1, 11)
]

image_styles = [
    ('15abacb7-475b-48c3-a81e-78d66a9e1a30', 4),
    ('1bb3eb27-85f9-4f63-9dc6-9343aa8ac96a', 3),
    ('3cb3d32d-f296-4eeb-944e-97c6bd7a2a19', 3),
    ('44acb21b-db2d-480f-a4b0-de4440d20c9e', 3),
    ('8c408ee8-ff22-4250-9e6c-5afe591c80ed', 4),
    ('af3b0581-938b-4f63-b3ae-f92ed099f289', 3),
    ('da12549a-5942-4352-9917-9b3ac7b2396f', 4),
    ('de7c7076-697c-49c4-9db2-9fa398b73de9', 4)
]

def get_embedding_string(text: str) -> str:
    print(f"Generating embedding for: {text}")
    req = urllib.request.Request(
        'http://localhost:8084/embeddings',
        method='POST',
        data=json.dumps({'text': text}).encode(),
        headers={'Content-Type': 'application/json'}
    )
    with urllib.request.urlopen(req) as response:
        data = json.loads(response.read().decode())
        vector = data['embedding']
        # Return as a string representation of the JSON array for MSSQL
        return f"'[{','.join(str(x) for x in vector)}]'"

sql_file = r'c:\work_spaces\pinterest_1000\projects\scripts\populate_img_data.sql'

with open(sql_file, 'w', encoding='utf-8') as f:
    f.write('USE [ImageDatabase];\nGO\n\n')

    # Categories
    f.write('INSERT INTO [dbo].[image_categories] (description, embedding)\nVALUES\n')
    for i, item in enumerate(categories):
        emb_str = get_embedding_string(item)
        ending = ';' if i == len(categories) - 1 else ','
        f.write(f"    ('{item}', {emb_str}){ending}\n")
    f.write('GO\n\n')

    # Room Types
    f.write('INSERT INTO [dbo].[image_room_type] (description, embedding)\nVALUES\n')
    for i, item in enumerate(room_types):
        emb_str = get_embedding_string(item)
        ending = ';' if i == len(room_types) - 1 else ','
        f.write(f"    ('{item}', {emb_str}){ending}\n")
    f.write('GO\n\n')

    # Styles
    f.write('INSERT INTO [dbo].[image_style] (description, embedding)\nVALUES\n')
    for i, item in enumerate(styles):
        emb_str = get_embedding_string(item)
        ending = ';' if i == len(styles) - 1 else ','
        f.write(f"    ('{item}', {emb_str}){ending}\n")
    f.write('GO\n\n')

    # Images
    f.write('INSERT INTO [dbo].[image] (id, category_id, room_type_id)\nVALUES\n')
    for i, item in enumerate(images):
        ending = ';' if i == len(images) - 1 else ','
        f.write(f"    ('{item[0]}', {item[1]}, {item[2]}){ending}\n")
    f.write('GO\n\n')

    # Image Styles
    f.write('INSERT INTO [dbo].[image_image_style] (image_id, style_id)\nVALUES\n')
    for i, item in enumerate(image_styles):
        ending = ';' if i == len(image_styles) - 1 else ','
        f.write(f"    ('{item[0]}', {item[1]}){ending}\n")
    f.write('GO\n\n')

print("Finished generating populate_img_data.sql!")
