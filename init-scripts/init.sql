CREATE TABLE IF NOT EXISTS hashes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    sha1 VARCHAR(250) NOT NULL,
    date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Ensure you're using the correct database
USE codetask;

-- Drop procedure if it already exists
DROP PROCEDURE IF EXISTS getHashes;

DELIMITER $$

-- Create the stored procedure
CREATE PROCEDURE getHashes()
BEGIN
    WITH hashes_resultset AS 
    (
        SELECT sha1, DATE_FORMAT(date, '%Y-%m-%d') AS `date` 
        FROM hashes
    )
    select
        date,
        count(sha1) as count
        from hashes_resultset
    group by date;
END $$

DELIMITER ;