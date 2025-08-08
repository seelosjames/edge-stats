import { useState, useContext } from "react";
import AuthContext from "./context/AuthContext";
import { Link } from "react-router-dom";

const Signup = () => {
	const authContext = useContext(AuthContext);
	if (!authContext) return null;
	const { registerUser } = authContext;

	const [email, setEmail] = useState<string>("");
	const [password, setPassword] = useState<string>("");

	const handleSubmit = (e: React.FormEvent) => {
		e.preventDefault();
		registerUser(email, password);
	};

	return (
		<div className="p-4 max-w-7xl mx-auto flex flex-col items-center justify-center flex-grow">
			<h2 className="text-4xl font-bold text-gray-800 mb-6">Create an Account</h2>
			<form onSubmit={handleSubmit} className="bg-white p-6 rounded-lg shadow-md w-full max-w-md">
				<input
					type="email"
					value={email}
					onChange={(e) => setEmail(e.target.value)}
					placeholder="Email"
					className="w-full p-3 border rounded-lg mb-4"
				/>
				<input
					type="password"
					value={password}
					onChange={(e) => setPassword(e.target.value)}
					placeholder="Password"
					className="w-full p-3 border rounded-lg mb-4"
				/>
				<button type="submit" className="w-full px-4 py-2 bg-blue-500 text-white rounded-lg">
					Sign Up
				</button>
			</form>
			<p className="mt-4 text-gray-600">
				Already have an account?{" "}
				<Link to="/login" className="text-blue-500">
					Login
				</Link>
			</p>
		</div>
	);
};

export default Signup;
