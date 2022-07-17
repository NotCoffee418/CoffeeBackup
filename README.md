# CoffeeBackup

CoffeeBackup is a backup service utilizing docker-compose volumes to back up to S3 or Storj.
This application was tested and developed for Ubuntu 20.04 but should work for other Linux distributions as well as Mac and Windows.

## Installation

## Setting up a storage provider

Backups can be securely stored in one of the following storage providers:

- [Amazon S3](https://aws.amazon.com/s3/)
- [Storj](https://storj.io/)

### Provider: Amazon S3
1. [Create](https://s3.console.aws.amazon.com/s3/bucket/create) an Amazon S3 bucket with versioning enabled to protect against overwrites.
3. Create a policy with the following permissions for the bucket:  
   - List: ListBucket
   - Write: PutObject
5. Optional: Create a lifecycle rule under Management > Lifecycle Rules that expires objects older than the desired amount of time.

### Provider: Storj

1. Create a Storj bucket [here](https://eu1.storj.io/buckets/creation) and store the passphrase securely.
This bucket should be used only for this instance of the application.
2. Create an "Access Grant" access grant for the bucket [here](https://eu1.storj.io/access-grants).
Make sure you select 'Create My Own Passphrase' and **use the bucket's passphrase**.
3. Add the permissions: `Write`, `List`.  
4. (not recommended) Add `Delete` permission if you want to auto-clean backups through the backup service.

## Setting up the backup service

### Prerequisites

Install [Docker](https://docs.docker.com/get-docker/) and the [Docker Compose](https://docs.docker.com/compose/install/) plugin.

### Configuration

Depending on your needs, you can set up a docker-compose.yml file with environment variables or use an appsettings.json file.  
For this guide, we'll use an appsettings file.

1. SSH into the server
2. `nano /etc/coffeebackup/appsettings.json`
3. Paste in the contents of [appsettings.example.json](https://raw.githubusercontent.com/NotCoffee418/CoffeeBackup/blob/main/appsettings.example.json)
4. Change settings to your preference:
   - **BackupIntervalDays:** Amount of days between each backup.
   - Configure one and only one storage location:
      - **S3:**
        - **AccessKeyId:** Your AWS access key ID.
		- **SecretAccessKey:** Your AWS secret access key.
		- **BackupBucketName:** The name of the bucket to store backups in. It should be exclusive for backups made by this instance.
		- **StorageClass**: The desired storage class for your backups. By default it uses standard infrequent access.
      - **Storj:**
        - **AccessGrantToken:** Access grant token for the Storj bucket.
        - **BackupBucketName:** Name of the Storj bucket.
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
    # In linux, the backup mount point is `/backup/`
    - /etc/nginx/sites-enabled/:/backup/etc/nginx/sites-enabled
    - /home/myusername/:/backup/home/myusername/
    - /home/anotheruser/specificfile.txt:/backup/home/myusername/specificfile.txt
```


## Auto-cleaning old backups

### Option 1: Using Amazon S3

Set up a lifecycle policy on your bucked to clean out or deep archive old backups through AWS.

### Option 2: Through the backup service (not recommended)

You can also clean up old backups using the backup service directly.
This is easier to set up, but not recommended in the event the server is compromised, a malicious actor can also remove backups.
Additionally, it is disabled for AWS for security reasons. This (currently) only works for Storj due to a lack of a better alternative.

1. Ensure that the access credentials used also have `delete` permission.
2. Modify the configuration through either the appsettings.json file or the docker-compose.yml file.
`nano /etc/coffeebackup/appsettings.json`

Add the optional setting `CleanupAfterDays` to BackupSettings with your preferred value. Don't forget the comma on the previous line.
```json
...
    "BackupSettings": {
        // How often are backups generated
        "BackupIntervalDays": 1,
        "CleanupAfterDays": 90
      },
...
```

## Troubleshooting
### FAQ
Q) Where are the logs?
A) `/var/log/coffeebackup/` on the server.