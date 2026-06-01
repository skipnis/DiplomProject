dev-up:
	docker compose -f docker-compose.dev.yml up --build -d

dev-up-fresh:
	docker compose -f docker-compose.dev.yml build --no-cache
	docker compose -f docker-compose.dev.yml up -d

dev-down:
	docker compose -f docker-compose.dev.yml down

dev-logs:
	docker compose -f docker-compose.dev.yml logs -f

prod-up:
	docker compose up --build -d

prod-up-fresh:
	docker compose build --no-cache
	docker compose up -d

prod-down:
	docker compose down

prod-logs:
	docker compose logs -f
