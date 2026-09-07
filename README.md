# altinn-verification-dis-poc
TEMP repo for core dis poc

## Deployment (Flux on DIS)

`verification` is a demo service in the **core** product, deployed to namespace
`product-core` in `at22` and `at23`. Modelled on
[info.altinn.no](https://github.com/Altinn/info.altinn.no) — a single app, a single
OCI artifact, no extra indirection.

```
syncroot/
  base/
    kustomization.yaml          # namespace: product-core
    verification/               # the app: deployment, service, httproute
  at22/kustomization.yaml       # image tag + hostname
  at23/kustomization.yaml
```

`.github/workflows/publish-syncroot.yaml` publishes `./syncroot` to ACR as
`core/syncroot:<env>`. The cluster-side Flux configuration — provisioned by the
`dis_products_syncroot_multitenancy` terraform module in the `core` repo — pulls
`oci://altinncr.azurecr.io/core/syncroot` at tag `<env>` and applies `./<env>` from
inside it. That module derives the artifact name, the tag, and the `product-core`
namespace from `product = "core"`, so don't rename them here in isolation.

### Building the image
`.github/workflows/docker-publish.yaml` builds `Dockerfile` and pushes to
`ghcr.io/altinn/altinn-verification`:

- push to `main` → `sha-<short>` and `latest`
- pull request → builds only, nothing pushed
- manual dispatch → optionally an extra tag of your choosing

Use the immutable `sha-<short>` tag when deploying; the run's job summary prints the
tags it produced.

### Deploying
Run the **Publish Syncroot artifact** workflow, picking an environment and the image
tag to deploy. The tag is written into `syncroot/<env>/kustomization.yaml` at publish
time, so no image tags are committed (`will-be-replaced` is the placeholder).

### Adding an environment
Add `syncroot/<env>/kustomization.yaml` and an option to the workflow's `environment`
input. The environment must also be onboarded in the `core` repo.

### Validate locally
```
kustomize build syncroot/at22
kustomize build syncroot/at23
```

### Before this reconciles
- **`core` needs onboarding as a product in both clusters** — one
  `dis_products_syncroot_multitenancy` call per environment in the `core` repo.
  Nothing else creates the `product-core` namespace or the Flux configuration.
- **The GHCR package must be readable by ACR.** The manifests pull the image through
  the ACR cache (`altinncr.azurecr.io/ghcr.io/...`), following info.altinn.no. A newly
  published GHCR package is private by default, so it needs either public visibility
  or a credential set on the `altinncr` cache rule before the first pull works.
- **This repo needs push rights to `altinncr`.** Add an entry to
  `infrastructure/syncroots/terraform.tfvars.json` in `Altinn/altinn-platform`:

  ```json
  "core": {
    "repo_name": "altinn-verification-dis-poc",
    "environments": [],
    "branches": ["main"]
  }
  ```

  That provisions a user-assigned managed identity, a GitHub OIDC federated
  credential for `repo:Altinn/altinn-verification-dis-poc:ref:refs/heads/main`,
  `AcrPush` on `altinncr`, and writes the three `DIS_SYNCROOT_AZURE_*` secrets into
  this repo. No GitHub App and no manually-managed secrets.

  The map key must be `core`: the ACR role assignment carries an ABAC condition
  restricting writes to repositories starting with `<product_name>/`, which is why
  the artifact is `core/syncroot`. The federated credential is scoped to `main`,
  which is what the workflow's `github.ref` gate matches.

### Demo shortcuts
- **No health probes.** The app exposes no `/health` endpoints, so the Deployment has
  none — probes would crash-loop it. Add both together.
- **No database.** `PostgreSqlSettings__EnableDBConnection: "false"` skips the EF
  migrations so the pod starts clean; the API cannot serve real traffic until
  connection strings come in from a Secret.
- **No Linkerd, no persistent storage, no autoscaling.**
- Hostnames `verification.<env>.dis-core.altinn.cloud` follow the pattern used by
  infoportal and dialogporten but are unverified.
