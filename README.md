# CoffeeBackup

Simple backup solution utilizing docker-compose mounts to back up to S3 or Storj.

## Installation

## Setting up a storage provider

Backups can be securely stored in one of the following storage providers:

- [Amazon S3](https://aws.amazon.com/s3/)
- [Storj](https://storj.io/)

### Provider: Amazon S3

1. [Create](https://s3.console.aws.amazon.com/s3/bucket/create) an Amazon S3 bucket with versioning enabled to protect against overwrites.
2. Create a policy with the following permissions for the bucket:  
   - List: ListBucket
   - Write: PutObject
3. Optional: Create a lifecycle rule under Management > Lifecycle Rules that expires objects older than the desired amount of time.

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

```bash
# Create & navigate to the config directory
mkdir /etc/coffeebackup && cd /etc/coffeebackup

# Download the config file
wget -O appsettings.json https://raw.githubusercontent.com/NotCoffee418/CoffeeBackup/main/appsettings.example.json

# Edit the config file
nano nano /etc/coffeebackup/appsettings.json
# CTRL+X to exit out of nano
```

Change settings to your preference:

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

### Setting up the docker-compose.yml file

This is where the magic happens. We will utilize Docker's volume feature to mount all locations we want to backup to the application's internal `/backup` directory.

```bash
# Navigate to your home directory, or wherever you want the docker-compose.yml file to be located.
mkdir ~/coffeebackup && cd ~/coffeebackup

# Download the example docker-compoes file
wget -O docker-compose.yml https://raw.githubusercontent.com/NotCoffee418/CoffeeBackup/main/docker-compose.example.yml

# Modify the docker-compose.yml file to match your needs.
nano docker-compose.yml
```

Remove the "backup volumes" example entries and add your own volumes to the `volumes`.

The only rule is that all volumes to be backed up should be internally mounted somewhere inside of the `/backup/` directory. Generally you will want to maintain the directory structure of the file system, as seen in the [example docker-compose.yml](https://github.com/NotCoffee418/CoffeeBackup/blob/main/docker-compose.example.yml).

### Running the backup service

```bash
# Pull the latest version of the image
docker pull notcoffee418/coffeebackup
docker-compose up -d
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