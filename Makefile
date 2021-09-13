.PHONY: help all create delete deploy check clean rrapi rrui rrprod ngsa lr reset-prometheus reset-grafana jumpbox

help :
	@echo "Usage:"
	@echo "   make all              - create a cluster and deploy the apps"
	@echo "   make create           - create a kind cluster"
	@echo "   make delete           - delete the kind cluster"
	@echo "   make deploy           - deploy the apps to the cluster"
	@echo "   make check            - curl endpoints cluster"
	@echo "   make clean            - delete deployments"
	@echo "   make rrapi            - build and deploy a local relayrunner backend docker image"
	@echo "   make rrui             - build and deploy a local relayrunner client docker image"
	@echo "   make rrprod           - deploy relayrunner images from registry"
	@echo "   make ngsa             - build and deploy a local ngsa-app docker image"
	@echo "   make lr               - build and deploy a local loderunner docker image"
	@echo "   make reset-prometheus - reset the Prometheus volume (existing data is deleted)"
	@echo "   make reset-grafana    - reset the Grafana volume (existing data is deleted)"
	@echo "   make jumpbox          - deploy a 'jumpbox' pod"

all : create deploy jumpbox check

delete :
	# delete the cluster (if exists)
	@# this will fail harmlessly if the cluster does not exist
	-k3d cluster delete

create : delete
	# create the cluster and wait for ready
	@# this will fail harmlessly if the cluster exists
	@# default cluster name is k3d

	@k3d cluster create --registry-use k3d-registry.localhost:5000 --config deploy/k3d.yaml --k3s-server-arg "--no-deploy=traefik" --k3s-server-arg "--no-deploy=servicelb"

	# wait for cluster to be ready
	@kubectl wait node --for condition=ready --all --timeout=60s
	@sleep 5
	@kubectl wait pod -A --all --for condition=ready --timeout=60s

deploy :
	# deploy ngsa-app
	@# continue on most errors
	-kubectl apply -f deploy/ngsa-memory

	# deploy loderunner after the ngsa-app starts
	@kubectl wait pod ngsa-memory --for condition=ready --timeout=30s
	-kubectl apply -f deploy/loderunner

	# Create backend image from codebase
	@docker build ./backend -t k3d-registry.localhost:5000/relayrunner-backend:local
	# Load new backend image to k3d registry
	@docker push k3d-registry.localhost:5000/relayrunner-backend:local
	# deploy local relayrunner backend
	-kubectl apply -f deploy/relayrunner-backend/local

	# Create client image from codebase
	@docker build ./client --target nginx-dev -t k3d-registry.localhost:5000/relayrunner-client:local
	# Load new client image to k3d registry
	@docker push k3d-registry.localhost:5000/relayrunner-client:local
	# deploy local relayrunner client
	-kubectl apply -f deploy/relayrunner-client/local

	# deploy prometheus and grafana
	-kubectl apply -f deploy/prometheus
	-kubectl apply -f deploy/grafana

	# deploy fluentbit
	-kubectl create secret generic log-secrets --from-literal=WorkspaceId=dev --from-literal=SharedKey=dev
	-kubectl apply -f deploy/fluentbit/account.yaml
	-kubectl apply -f deploy/fluentbit/log.yaml
	-kubectl apply -f deploy/fluentbit/stdout-config.yaml
	-kubectl apply -f deploy/fluentbit/fluentbit-pod.yaml

	# wait for pods to start
	@kubectl wait pod -A --all --for condition=ready --timeout=60s

	# display pod status
	-kubectl get po -A | grep "default\|monitoring"

check :
	# curl all of the endpoints
	@echo "\nchecking ngsa-memory..."
	@curl localhost:30080/version
	@echo "\n\nchecking loderunner..."
	@curl localhost:30088/version
	@echo "\n\nchecking prometheus..."
	@curl localhost:30000
	@echo "\nchecking grafana..."
	@curl localhost:32000
	@echo "\nchecking relayrunner client..."
	@curl localhost:32080
	@echo "\n\nchecking relayrunner backend..."
	@curl localhost:32088/version

clean :
	# delete the deployment
	@# continue on error
	-kubectl delete -f deploy/loderunner --ignore-not-found=true
	-kubectl delete -f deploy/ngsa-memory --ignore-not-found=true
	-kubectl delete ns monitoring --ignore-not-found=true
	-kubectl delete -f deploy/fluentbit/fluentbit-pod.yaml --ignore-not-found=true
	-kubectl delete -f deploy/relayrunner-client/local --ignore-not-found=true
	-kubectl delete -f deploy/relayrunner-backend/local --ignore-not-found=true
	-kubectl delete secret log-secrets --ignore-not-found=true

	# show running pods
	@kubectl get po -A

rrapi:
	# build the local image and load into k3d
	docker build ./backend -t k3d-registry.localhost:5000/relayrunner-backend:local
	docker push k3d-registry.localhost:5000/relayrunner-backend:local

	# delete/deploy the local relayrunner backend
	-kubectl delete -f deploy/relayrunner-backend/local --ignore-not-found=true
	kubectl apply -f deploy/relayrunner-backend/local

	# wait for pod to be ready
	@sleep 5
	@kubectl wait pod relayrunner-backend --for condition=ready --timeout=30s

	@kubectl get po

	# display the relayrunner version
	-http localhost:32088/version

rrui:
	# build the local image and load into k3d
	docker build ./client --target nginx-dev -t k3d-registry.localhost:5000/relayrunner-client:local
	docker push k3d-registry.localhost:5000/relayrunner-client:local

	# delete/deploy local relayrunner client
	-kubectl delete -f deploy/relayrunner-client/local --ignore-not-found=true
	kubectl apply -f deploy/relayrunner-client/local

	# wait for pod to be ready
	@sleep 5
	@kubectl wait pod relayrunner-client --for condition=ready --timeout=30s

	@kubectl get po

	# hit the relayrunner client endpoint
	-http -h localhost:32080

rrprod:
	# delete local backend relayrunner pod and deploy image from ghcr
	-kubectl delete -f deploy/relayrunner-backend/local --ignore-not-found=true
	kubectl apply -f deploy/relayrunner-backend

	# wait for pod to be ready
	@sleep 5
	@kubectl wait pod relayrunner-backend --for condition=ready --timeout=30s

	# delete local client relayrunner pod and deploy image from ghcr
	-kubectl delete -f deploy/relayrunner-client/local --ignore-not-found=true
	kubectl apply -f deploy/relayrunner-client

	# wait for pod to be ready
	@sleep 5
	@kubectl wait pod relayrunner-client --for condition=ready --timeout=30s

	@kubectl get po

	# display the relayrunner version
	-http localhost:32088/version

	# hit the relayrunner client endpoint
	-http -h localhost:32080

ngsa :
	# build the local image of ngsa-app and load into k3d
	docker build ../ngsa-app -t k3d-registry.localhost:5000/ngsa-app:local
	docker push k3d-registry.localhost:5000/ngsa-app:local

	# delete loderunner
	-kubectl delete -f deploy/loderunner --ignore-not-found=true

	# delete/deploy the ngsa-app
	-kubectl delete -f deploy/ngsa-memory --ignore-not-found=true
	kubectl apply -f deploy/ngsa-local

	# deploy loderunner after app starts
	@kubectl wait pod ngsa-memory --for condition=ready --timeout=30s
	@sleep 5
	kubectl apply -f deploy/loderunner
	@kubectl wait pod loderunner --for condition=ready --timeout=30s

	@kubectl get po

	# display the ngsa-app version
	-http localhost:30080/version

lr :
	# build the local image of loderunner and load into k3d
	docker build ../loderunner -t k3d-registry.localhost:5000/ngsa-lr:local
	docker push k3d-registry.localhost:5000/ngsa-lr:local
	
	# delete / create loderunner
	-kubectl delete -f deploy/loderunner --ignore-not-found=true
	kubectl apply -f deploy/loderunner-local
	kubectl wait pod loderunner --for condition=ready --timeout=30s
	@kubectl get po

	# display the current version
	-http localhost:30088/version

reset-prometheus :
	# remove and create the /prometheus volume
	@sudo rm -rf /prometheus
	@sudo mkdir -p /prometheus
	@sudo chown -R 65534:65534 /prometheus

reset-grafana :
	# remove and copy the data to /grafana volume
	@sudo rm -rf /grafana
	@sudo mkdir -p /grafana
	@sudo cp -R deploy/grafanadata/grafana.db /grafana
	@sudo chown -R 472:472 /grafana

jumpbox :
	# start a jumpbox pod
	@-kubectl delete pod jumpbox --ignore-not-found=true

	@kubectl run jumpbox --image=ghcr.io/retaildevcrews/alpine --restart=Always -- /bin/sh -c "trap : TERM INT; sleep 9999999999d & wait"
	@kubectl wait pod jumpbox --for condition=ready --timeout=30s

	# Run an interactive bash shell in the jumpbox
	# kj
	# use kje <command>
	# kje http ngsa-memory:8080/version