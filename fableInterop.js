function triggerPrompt(message) {
    return prompt(message);
}

function triggerAlert(message) {
	alert(message);
}

function triggerConfirm(message) {
	return confirm(message);
}

export { triggerPrompt, triggerAlert, triggerConfirm };