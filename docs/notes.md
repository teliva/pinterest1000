# Notes
- Images will have a one-to-one relationship with category and room-type, however it will have a one-to-many relationship style
- Dan and I decided that it is quite unlikely that an image will use more than two styles.

- Into the keyword generation model you can input your dictionary:

##Your master database list
```
my_database_vocabulary = ["Kitchen", "Bedroom", "Bathroom", "Modern", "Bohemian"]

keywords = kw_model.extract_keywords(
    "I want a mid-century modern kitchen with an island",
    seed_keywords=my_database_vocabulary # Nudges KeyBERT to prioritize these concepts
)
```