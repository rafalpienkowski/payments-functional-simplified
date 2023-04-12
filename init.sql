CREATE SCHEMA IF NOT EXISTS simplified AUTHORIZATION CURRENT_USER;
CREATE TABLE simplified.transactions (
     transaction_id uuid NOT NULL,
     customer_id uuid NOT NULL,
     started_at timestamp NOT NULL,
     provider_reference varchar NOT NULL,
     amount numeric NOT NULL,
     status varchar NOT NULL,
     finished_at timestamp NULL,
     PRIMARY KEY (transaction_id)
);

CREATE UNIQUE INDEX idx_transactions_transaction_id ON simplified.transactions(transaction_id);