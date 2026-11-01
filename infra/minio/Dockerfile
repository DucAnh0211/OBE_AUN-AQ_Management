# Build the upstream source release: the old public prebuilt image is no longer pullable.
FROM golang:1.24.8-alpine AS build
ENV CGO_ENABLED=0 GOMAXPROCS=2 GOFLAGS=-p=2
RUN go install github.com/minio/minio@RELEASE.2025-10-15T17-29-55Z

FROM debian:bookworm-slim
COPY --from=build /etc/ssl/certs/ca-certificates.crt /etc/ssl/certs/ca-certificates.crt
COPY --from=build /go/bin/minio /usr/local/bin/minio
RUN mkdir /data && chown 10001:10001 /data
USER 10001:10001
EXPOSE 9000 9001
ENTRYPOINT ["minio"]
CMD ["server", "/data", "--console-address", ":9001"]
