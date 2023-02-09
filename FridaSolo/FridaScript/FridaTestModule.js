include("./FridaModule002.js")
function main() {
	console.log("Script run.");
	var m2 = new module002("module002 args");
}
setImmediate(main);

class Father {
    constructor(money) {
        this.money = 1000000
    }
}
class Son extends Father {
    constructor() {
        super()
    }
}
const S1 = new Son()