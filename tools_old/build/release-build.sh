#!/bin/sh
# Build for release, and if all unit tests pass, create a GitHub release,
# upload the binaries, and build a Docker container.

# Required Enviroment Variables
# GH_ORGANIZATION_NAME - The GitHub organization (or username) the repository belongs to. 
# GH_REPO_NAME - The repository where the build should be created.
# VERSION_NUMBER - Semantic version number.
# PROJECT_NAME - Project name
# DOCKER_USERNAME - Docker login ID for publishing images
# DOCKER_PASSWORD - Docker password for publishing images
# DOCKER_REGISTRY - Hostname of the NCI Docker registry.
# GITHUB_TOKEN - Github access token for creating releases and uploading build artifacts.


if [ -z "$GH_ORGANIZATION_NAME" ]; then echo GH_ORGANIZATION_NAME not set; exit 1; fi
if [ -z "$GH_REPO_NAME" ]; then echo GH_REPO_NAME not set; exit 1; fi
if [ -z "$VERSION_NUMBER" ]; then echo VERSION_NUMBER not set; exit 1; fi
if [ -z "$PROJECT_NAME" ]; then echo PROJECT_NAME not set; exit 1; fi
if [ -z "$DOCKER_USERNAME" ]; then echo DOCKER_USERNAME not set; exit 1; fi
if [ -z "$DOCKER_PASSWORD" ]; then echo DOCKER_PASSWORD not set; exit 1; fi
if [ -z "$DOCKER_REGISTRY" ]; then echo DOCKER_REGISTRY not set; exit 1; fi
if [ -z "$GITHUB_TOKEN" ]; then echo GITHUB_TOKEN not set; exit 1; fi


export SCRIPT_PATH="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
export PROJECT_HOME="$(cd $SCRIPT_PATH/../.. && pwd)"
export TEST_ROOT=${PROJECT_HOME}/test
export CURDIR=`pwd`

export IMAGE_NAME="${DOCKER_REGISTRY}/ocpl/bestbets-api"

echo Creating Release Build.



# Go to the project home foldder and restore packages
cd $PROJECT_HOME
echo Restoring packages
dotnet restore


# Build and run unit tests.
ERRORS=0
echo Executing unit tests
for test in $(ls -d ${TEST_ROOT}/*/); do
    dotnet test $test

    # Check for errors
    if [ $? != 0 ]; then
        export ERRORS=1
    fi
done

# If any unit tests failed, abort the operation.
if [ $ERRORS == 1 ]; then
    echo Errors have occured.
    exit 1
fi

#===================================================================================
#  BEST BETS API
#===================================================================================

# Publish API to temporary location and create archives for uploading to GitHub
API_TMPDIR=`mktemp -d` || exit 1
dotnet publish src/NCI.OCPL.Api.BestBets/ -o $API_TMPDIR

# Creating the archive in the publishing folder prevents the parent directory being included
# in the archive.
echo "Creating release archive"
cd $API_TMPDIR
zip -r project-release.zip .
cd $PROJECT_HOME





#===================================================================================
#  BEST BETS INDEXER
#===================================================================================

# Publish API to temporary location and create archives for uploading to GitHub
INDEXER_TMPDIR=`mktemp -d` || exit 1
dotnet publish src/NCI.OCPL.Api.BestBets.Indexer/ -o $INDEXER_TMPDIR

# Creating the archive in the publishing folder prevents the parent directory being included
# in the archive.
echo "Creating release archive"
cd $INDEXER_TMPDIR
zip -r project-release.zip .
cd $PROJECT_HOME



#===================================================================================
#  Create GitHub release with build artifacts.
#===================================================================================
echo "Creating release '${VERSION_NUMBER}' in github"
github-release release --user ${GH_ORGANIZATION_NAME} --repo ${GH_REPO_NAME} --tag ${VERSION_NUMBER} --name "${VERSION_NUMBER}"
if [ $? != 0 ]; then echo Exiting with errors; exit 1; fi


echo "Uploading BestBets API artifacts into github"
github-release upload --user ${GH_ORGANIZATION_NAME} --repo ${GH_REPO_NAME} --tag ${VERSION_NUMBER} --name "${PROJECT_NAME}-${VERSION_NUMBER}.zip" --file $API_TMPDIR/project-release.zip
if [ $? != 0 ]; then echo Exiting with errors; exit 1; fi

echo "Uploading BestBets Indexer artifacts to github"
github-release upload --user ${GH_ORGANIZATION_NAME} --repo ${GH_REPO_NAME} --tag ${VERSION_NUMBER} --name "bestbets-Indexer-${VERSION_NUMBER}.zip" --file $INDEXER_TMPDIR/project-release.zip
if [ $? != 0 ]; then echo Exiting with errors; exit 1; fi




#===================================================================================
#  Create and publish Docker images
#===================================================================================
docker login -u $DOCKER_USERNAME -p $DOCKER_PASSWORD $DOCKER_REGISTRY
if [ $? -ne 0 ]; then echo "Error logging into '$DOCKER_REGISTRY' as '$DOCKER_USERNAME'."; exit 1; fi

# Create and push SDK image
docker build -q --no-cache --build-arg version_number=${VERSION_NUMBER} -t ${IMAGE_NAME}:sdk -t ${IMAGE_NAME}:sdk-${VERSION_NUMBER} -f src/NCI.OCPL.Api.BestBets/Dockerfile/Dockerfile.SDK .
if [ $? -ne 0 ]; then echo "Error building image '${IMAGE_NAME}:sdk-${VERSION_NUMBER}'."; exit 1; fi
eval $SCRIPT_PATH/publish-docker-image.sh ${IMAGE_NAME} sdk
eval $SCRIPT_PATH/publish-docker-image.sh ${IMAGE_NAME} sdk-${VERSION_NUMBER}

# Create and push Runtime image
docker build -q --no-cache --build-arg version_number=${VERSION_NUMBER} -t ${IMAGE_NAME}:runtime -t ${IMAGE_NAME}:runtime-${VERSION_NUMBER} -f src/NCI.OCPL.Api.BestBets/Dockerfile/Dockerfile.Runtime .
if [ $? -ne 0 ]; then echo "Error building image '${IMAGE_NAME}:runtime-${VERSION_NUMBER}'."; exit 1; fi
eval $SCRIPT_PATH/publish-docker-image.sh ${IMAGE_NAME} runtime
eval $SCRIPT_PATH/publish-docker-image.sh ${IMAGE_NAME} runtime-${VERSION_NUMBER}

