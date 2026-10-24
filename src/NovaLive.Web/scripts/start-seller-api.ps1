# Start the existing API for Seller LOCAL integration. No backend source changes.
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$envFile = Join-Path $repo '.env'
if (!(Test-Path $envFile)) { throw 'Create the root .env from .env.example first.' }
$settings = @{}
foreach ($line in Get-Content $envFile) {
    if ($line -match '^([A-Z][A-Z0-9_]*)=(.*)$') {
        $settings[$Matches[1]] = $Matches[2].Trim().Trim('"').Trim("'")
    }
}
foreach ($key in @('POSTGRES_PORT','POSTGRES_DB','POSTGRES_USER','POSTGRES_PASSWORD','REDIS_PORT',
    'RABBITMQ_DEFAULT_USER','RABBITMQ_DEFAULT_PASS','JWT_SECRET','OTP_PEPPER','MINIO_API_PORT','MINIO_ROOT_USER','MINIO_ROOT_PASSWORD')) {
    if (!$settings[$key]) { throw "Missing .env key: $key" }
}
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ConnectionStrings__DefaultConnection = "Host=127.0.0.1;Port=$($settings.POSTGRES_PORT);Database=$($settings.POSTGRES_DB);Username=$($settings.POSTGRES_USER);Password=$($settings.POSTGRES_PASSWORD)"
$env:Redis__ConnectionString = "127.0.0.1:$($settings.REDIS_PORT),abortConnect=false"
$env:RabbitMQ__Host = 'localhost'
$env:RabbitMQ__Username = $settings.RABBITMQ_DEFAULT_USER
$env:RabbitMQ__Password = $settings.RABBITMQ_DEFAULT_PASS
$env:Jwt__Secret = $settings.JWT_SECRET
$env:Otp__Pepper = $settings.OTP_PEPPER
$env:Email__Provider = 'Logging'
$env:Email__LogOtp = 'false'
$env:Cookies__Secure = 'false' # HTTP loopback Development only; production unchanged.
$env:Minio__Endpoint = "127.0.0.1:$($settings.MINIO_API_PORT)"
$env:Minio__AccessKey = $settings.MINIO_ROOT_USER
$env:Minio__SecretKey = $settings.MINIO_ROOT_PASSWORD
$env:Minio__UseSsl = 'false'
$env:Minio__CdnBaseUrl = "http://localhost:$($settings.MINIO_API_PORT)"
# Never invoke external email, shipping or payment services in local QA.
$env:Ghn__BaseUrl = 'http://127.0.0.1:59999'
$env:Ghn__ApiToken = ''
$env:Ghn__ShopId = '0'
$env:Momo__Endpoint = 'http://127.0.0.1:59999'
$env:MOMO_ENDPOINT = 'http://127.0.0.1:59999'
$env:VietQr__BaseUrl = 'http://127.0.0.1:59999'
$env:VIETQR_BASE_URL = 'http://127.0.0.1:59999'
Push-Location $repo
try {
    dotnet run --no-launch-profile --project src/NovaLive.Api/NovaLive.Api.csproj --urls http://localhost:5001
    if ($LASTEXITCODE) { throw "NovaLive.Api exited with code $LASTEXITCODE" }
} finally { Pop-Location }
