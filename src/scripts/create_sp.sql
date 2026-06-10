USE ImageDatabase;
GO

CREATE OR ALTER PROCEDURE dbo.sp_FindBestMatches
    @embeddings_json NVARCHAR(MAX)  -- JSON array of embedding arrays: [[f1,...,f384],[f1,...,f384],...]
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #keyword_matches (
        category_id     INT   NULL,
        category_score  FLOAT NULL,
        room_type_id    INT   NULL,
        room_type_score FLOAT NULL,
        style_id        INT   NULL,
        style_score     FLOAT NULL
    );

    DECLARE @embedding NVARCHAR(MAX);
    DECLARE @sql       NVARCHAR(MAX);

    DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
        SELECT [value] FROM OPENJSON(@embeddings_json);

    OPEN cur;
    FETCH NEXT FROM cur INTO @embedding;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @sql = N'
            INSERT INTO #keyword_matches
            SELECT
                c.category_id,
                c.category_score,
                r.room_type_id,
                r.room_type_score,
                s.style_id,
                s.style_score
            FROM
                (SELECT TOP 1 category_id,
                        VECTOR_DISTANCE(''cosine'', embedding, CAST(''' + @embedding + N''' AS VECTOR(384))) AS category_score
                 FROM dbo.image_categories
                 WHERE embedding IS NOT NULL
                 ORDER BY category_score) c
            CROSS JOIN
                (SELECT TOP 1 room_type_id,
                        VECTOR_DISTANCE(''cosine'', embedding, CAST(''' + @embedding + N''' AS VECTOR(384))) AS room_type_score
                 FROM dbo.image_room_type
                 WHERE embedding IS NOT NULL
                 ORDER BY room_type_score) r
            CROSS JOIN
                (SELECT TOP 1 style_id,
                        VECTOR_DISTANCE(''cosine'', embedding, CAST(''' + @embedding + N''' AS VECTOR(384))) AS style_score
                 FROM dbo.image_style
                 WHERE embedding IS NOT NULL
                 ORDER BY style_score) s;
        ';
        EXEC sp_executesql @sql;

        FETCH NEXT FROM cur INTO @embedding;
    END;

    CLOSE cur;
    DEALLOCATE cur;

    -- Return the best (lowest cosine distance) match per entity type across all keywords
    SELECT
        (SELECT TOP 1 category_id    FROM #keyword_matches ORDER BY category_score)   AS BestCategoryId,
        (SELECT TOP 1 category_score FROM #keyword_matches ORDER BY category_score)   AS BestCategoryScore,
        (SELECT TOP 1 room_type_id   FROM #keyword_matches ORDER BY room_type_score)  AS BestRoomTypeId,
        (SELECT TOP 1 room_type_score FROM #keyword_matches ORDER BY room_type_score) AS BestRoomTypeScore,
        (SELECT TOP 1 style_id       FROM #keyword_matches ORDER BY style_score)      AS BestStyleId,
        (SELECT TOP 1 style_score    FROM #keyword_matches ORDER BY style_score)      AS BestStyleScore;

    DROP TABLE #keyword_matches;
END;
GO
