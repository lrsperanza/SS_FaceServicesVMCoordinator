# this app is fetched by a Virtual Machine from the repository below, so to publish it suffices to just run this ps1. Remember to publish from Visual Studio before running this, as this will simply copy the published app from bin

$name = "queue_triggered_app"
$image = $name + ":latest"
$repo = "us-east1-docker.pkg.dev/lesser/lesser-repository/" + $image

docker build --pull --rm -f "GCLOUD.Dockerfile" -t $repo "."
#docker tag $image $repo
docker push $repo