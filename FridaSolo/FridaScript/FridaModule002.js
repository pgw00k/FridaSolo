
import './FridaModule001'
function module002(data)
{
	this.base = new module001();
	console.log("module002 init.");
	console.log(data);
}