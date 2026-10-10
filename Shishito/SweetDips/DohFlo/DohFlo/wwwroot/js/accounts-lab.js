const { createApp } = Vue;

createApp({
    data() {
        return {
            accounts: [],
            loading: false,
            errorMessage: "",
            showClosed: true
        };
    },

    computed: {
        visibleAccounts() {
            if (this.showClosed) {
                return this.accounts;
            }

            return this.accounts.filter(account => !account.isClosed);
        }
    },

    methods: {
        async loadAccounts() {
            this.loading = true;
            this.errorMessage = "";

            try {
                const response = await fetch("/api/accounts", {
                    headers: { "Accept": "application/json" }
                });

                if (!response.ok) {
                    throw new Error(`The API returned status ${response.status}.`);
                }

                const data = await response.json();
                this.accounts = data.map(account => ({
                    ...account,
                    saving: false
                }));
            } catch (error) {
                console.error(error);
                this.errorMessage = "DohFlo could not load the accounts. Check the API and try again.";
            } finally {
                this.loading = false;
            }
        },

        async changeStatus(account) {
            account.saving = true;
            this.errorMessage = "";

            try {
                const tokenResponse = await fetch("/api/security/antiforgery", {
                    headers: { "Accept": "application/json" }
                });

                if (!tokenResponse.ok) {
                    throw new Error(`Could not get the antiforgery token: ${tokenResponse.status}.`);
                }

                const tokens = await tokenResponse.json();

                if (!tokens.requestToken) {
                    throw new Error("The antiforgery token is missing.");
                }

                const response = await fetch(`/api/accounts/${account.id}/status`,
                    {
                        method: "PATCH",
                        headers: {
                            "X-CSRF-TOKEN": tokens.requestToken,
                            "Content-Type": "application/json",
                            "Accept": "application/json"
                        },
                        body: JSON.stringify({
                            isClosed: !account.isClosed
                        })
                    });

                if (!response.ok) {
                    throw new Error(`The API returned status ${response.status}.`);
                }

                const updatedAccount = await response.json();
                Object.assign(account, updatedAccount, { saving: false });
            } catch (error) {
                console.error(error);
                account.saving = false;
                this.errorMessage = "DohFlo could not change the account status.";
            }
        }
    },

    mounted() {
        this.loadAccounts();
    }
}).mount("#accounts-app");