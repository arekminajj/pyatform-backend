create or replace function top_users(
   top_limit int
)
RETURNS TABLE(UserId text, ProfilePictureUrl text, ChallengesCount bigint)
language plpgsql    
as $$
begin
    RETURN QUERY
    SELECT u."Id", u."ProfilePictureUrl", Count(DISTINCT c."Id") FROM "AspNetUsers" u 
    LEFT JOIN "Solutions" s ON s."UserId" = u."Id" AND s."HasPassedTests" = true
    LEFT JOIN "Challenges" c ON c."Id" = s."ChallengeId" 
    GROUP BY u."Id"
    ORDER BY Count(DISTINCT c."Id") DESC
    LIMIT top_limit;
end;$$;
