import { BrowserRouter, Route, Routes } from "react-router-dom";
import Signup from "./Signup";
import Login from "./Login";
import Landing from "./Landing";
import Header from "./Header";
import Dashboard from "./TestPrivateRoute";
import { AuthProvider } from "./context/AuthContext";
import PrivateRoute from "./PrivateRoute";

function App() {
	return (
		<BrowserRouter>
			<AuthProvider>
				<div className="flex flex-col min-h-screen bg-background">
					<Header />
					<main className="flex-grow flex flex-col overflow-y-auto">
						<Routes>
							<Route path="/" element={<Landing />} />
							<Route path="/signup" element={<Signup />} />
							<Route path="/login" element={<Login />} />
							<Route element={<PrivateRoute />}>
								<Route path="/testprivateroute" element={<Dashboard />} />
							</Route>
						</Routes>
					</main>
				</div>
			</AuthProvider>
		</BrowserRouter>
	);
}

export default App;
