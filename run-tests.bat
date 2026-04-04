@echo off
cd /d d:\RentACar\Rent-a-Car
dotnet test Tests\Tests.csproj --filter RequestServiceTests
pause
