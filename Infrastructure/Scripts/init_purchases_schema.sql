CREATE SCHEMA IF NOT EXISTS purchases;

CREATE TABLE IF NOT EXISTS purchases.suppliers (
    id SERIAL PRIMARY KEY,
    company_id INT NOT NULL,
    code VARCHAR(50) NOT NULL,
    name VARCHAR(255) NOT NULL,
    active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_suppliers_company_code UNIQUE (company_id, code)
);

CREATE TABLE IF NOT EXISTS purchases.purchase_orders (
    id SERIAL PRIMARY KEY,
    company_id INT NOT NULL,
    code VARCHAR(50) NOT NULL,
    supplier_id INT NOT NULL REFERENCES purchases.suppliers(id),
    warehouse_code VARCHAR(50) NOT NULL,
    status INT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    confirmed_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_purchase_orders_company_code UNIQUE (company_id, code)
);

CREATE TABLE IF NOT EXISTS purchases.purchase_order_items (
    id SERIAL PRIMARY KEY,
    purchase_order_id INT NOT NULL REFERENCES purchases.purchase_orders(id) ON DELETE CASCADE,
    product_code VARCHAR(50) NOT NULL,
    quantity INT NOT NULL CHECK (quantity > 0)
);

CREATE INDEX IF NOT EXISTS ix_purchase_orders_company_status ON purchases.purchase_orders(company_id, status);
CREATE INDEX IF NOT EXISTS ix_purchase_orders_created_at ON purchases.purchase_orders(created_at DESC);
