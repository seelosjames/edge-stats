import { useState, useContext } from "react";
import AuthContext from "./context/AuthContext";
import { Link } from "react-router-dom";

const Login = () => {
	const authContext = useContext(AuthContext);
	if (!authContext) return null;
	const { loginUser } = authContext;

	const [email, setEmail] = useState<string>("");
	const [password, setPassword] = useState<string>("");

	const handleSubmit = (e: React.FormEvent) => {
		e.preventDefault();
		loginUser(email, password);
	};

	return (
		<div className="flex flex-col items-center justify-center min-h-screen bg-gray-100 p-6">
			<h2 className="text-4xl font-bold text-gray-800 mb-6">Login</h2>
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
					Login
				</button>
			</form>
			<p className="mt-4 text-gray-600">
				Don't have an account?{" "}
				<Link to="/signup" className="text-blue-500">
					Sign Up
				</Link>
			</p>
		</div>
	);
};

export default Login;
