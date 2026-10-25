FROM cgr.dev/chainguard/minio:latest

EXPOSE 10000

CMD ["server", "/data", "--address", ":10000", "--console-address", ":10001"]
