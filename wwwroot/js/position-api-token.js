document.addEventListener("DOMContentLoaded", function () {
    const button = document.getElementById("getApiTokenButton")
    const loading = document.getElementById("apiTokenLoading")
    const result = document.getElementById("apiTokenResult")
    const input = document.getElementById("apiTokenInput")
    const copyButton = document.getElementById("copyApiTokenButton")
    const error = document.getElementById("apiTokenError")

    if (!button) {
        return
    }

    button.addEventListener("click", async function () {
        button.disabled = true
        button.classList.add("d-none")

        loading.classList.remove("d-none")
        result.classList.add("d-none")
        error.classList.add("d-none")

        try {
            const response = await fetch(
                `${window.location.pathname}?handler=ApiToken&positionId=${window.positionId}`
            )

            const data = await response.json()

            loading.classList.add("d-none")

            if (!response.ok) {
                throw new Error(data.message || "Failed to get API token")
            }

            input.value = data.token
            result.classList.remove("d-none")
        } catch (e) {
            loading.classList.add("d-none")

            error.textContent = e.message
            error.classList.remove("d-none")

            button.classList.remove("d-none")
            button.disabled = false
        }
    })

    copyButton.addEventListener("click", async function () {
        await navigator.clipboard.writeText(input.value)

        const oldText = copyButton.textContent
        copyButton.textContent = "Copied"

        setTimeout(function () {
            copyButton.textContent = oldText
        }, 1500)
    })
})