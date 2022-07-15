# CoffeeBackup

Backup utility with for Storj storage and Docker Compose to define backup locations.

This application was tested and developed for Ubuntu 20.04 but should work for other Linux distributions and Windows.


## Installation

## Setting up prerequisites

- [Storj account](https://storj.io/)
- [Azure account](https://azure.microsoft.com/en-us/free/) (Optional, for securely cleaning up old backups)
- [Docker](https://www.docker.com/)
- [Docker Compose plugin](https://docs.docker.com/compose/install/)

## Setting up Storj

1. Create a Storj bucket [here](https://eu1.storj.io/buckets/creation) and store the passphrase securely.
This bucket should be used only for this instance of the application.
2. Create two "Access Grant" access grants for the Storj bucket [here](https://eu1.storj.io/access-grants) and access grant token.
Make sure you elect 'Create My Own Passphrase' and **use the bucket's passphrase**.
   - Name: 'SERVERNAME-backup-service' with Permissions: `Write`, `List`
     - Add `Delete` permission if you want to auto-clean backups without Azure Functions.
   - Name: 'SERVERNAME-backup-cleanup' with Permissions: `Delete`, `List`
     - Only when you intend to auto-clean backups with Azure Functions.

## Setting up the backup service

Depending on your needs, you can set up a docker-compose.yml file with environment variables or use an appsettings.json file.  
For this guide, we'll use an appsettings file.

### Setting up the appsettings.json file
1. SSH into the server
2. `nano /etc/coffeebackup/appsettings.json`
3. Paste in the contents of [appsettings.service.example.json](https://github.com/NotCoffee418/CoffeeBackup/appsettings/appsettings.service.example.json])
4. Change settings to your preference:
   - **BackupIntervalDays:** Amount of days between each backup.
   - **StorjAccessGrant:** The long string of characters provided when creating the 'SERVERNAME-backup-service' earlier.
   - **StorjBackupBucket:** Name of the bucket.
5. CTRL+X to save and exit out.

### Setting up the docker-compose.yml file
This is where the magic happens. We will utilize Docker's volume feature to mount all locations we want to backup to the application's internal `/backup` directory.
1. Navigate to your home folder with `cd ~` (or wherever you want to store docker-compose.yml)
2. `nano docker-compose.yml`
3. Paste in the contents of [docker-compose.example.yml](https://raw.githubusercontent.com/NotCoffee418/CoffeeBackup/main/docker-compose.example.yml)
  Or download it locally and upload it to the server after modifying.
2. Add volumes to the `volumes` section.

The only rule is that all volumes to be backed up should be internally mounted somewhere inside of the `/backup/` directory.
Here is an example of a working configuration:

TODO: MODIFY THIS TO DOCKERHUB VERSION!
```yaml
version: '3.8'

services:
  coffeebackup:
    container_name: coffeebackup
    restart: unless-stopped
    build: # change to dockerhub version
      context: .
      dockerfile: CoffeeBackup.Service/Dockerfile 
    volumes:
    # -- Application volumes (Leave these)
    - /etc/coffeebackup/appsettings.json:/app/appsettings.json
    - /var/log/coffeebackup/:/app/logs/

    # -- Backup Volumes
    # All paths that require backup should be defined here
    - /etc/nginx/sites-enabled/:/backup/etc/nginx/sites-enabled
    - /home/myusername/:/backup/home/myusername/
    - /home/anotheruser/specificfile.txt:/backup/home/myusername/specificfile.txt
```


## Auto-cleaning old backups

### Option 1: Using Azure functions

With this option we will set up an Azure function to run periodically and delete any outdated backups.
The cost of this function is (to be tested) about $0.02 per month.

(( Unfinished ))

### Option 2: Through the backup service (NOT RECOMMENDED)

You can also clean up old backups using the backup service directly.
This is easier to set up, but not recommended in the event the server is compromised, a malicious actor can also remove backups.

1. Ensure your access grant for 'SERVERNAME-backup-service' has delete permissions, otherwise recreate it.
2. Modify the configuration through either the appsettings.json file or the docker-compose.yml file.
`nano /etc/coffeebackup/appsettings.json`

Add `CleanupAfterDays` with your preferred value like so. Don't forget the comma on the previous line.
```json
...
  "StorjAccessGrant": "",
  "StorjBackupBucket": "",
  "CleanupAfterDays": 30
}
```

## Troubleshooting
### FAQ
Q) Where are the logs?
A) `/var/log/coffeebackup/` on the server.