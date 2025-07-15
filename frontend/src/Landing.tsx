import { Link } from "react-router-dom";

function Landing() {
	return (
		<div className="min-h-screen bg-gray-100 text-gray-900 p-6 flex flex-col items-center justify-evenly">
			<div className="flex flex-col items-center justify-center mt-16">
				{/* Main Logo Section */}
				<div className="flex items-center gap-8 mb-6">
					<img src="/logos/react.svg" alt="React Logo" className="h-64 w-64 object-contain" />
					<span className="text-6xl font-bold text-gray-700">+</span>
					<img src="/logos/django.svg" alt="Django Logo" className="h-64 w-64 object-contain" />
				</div>

				{/* Headline */}
				<h1 className="text-5xl font-extrabold text-center mb-4">Welcome to My React/Django Template</h1>

				{/* Subtitle */}
				<p className="text-lg text-gray-600 text-center max-w-2xl mb-8">
					A modern, scalable, and efficient starter template for building full-stack applications with React and Django. Also includes the
					following technologies:
				</p>

				{/* Additional Tech Logos Section */}
				<div className="flex items-center gap-6 mt-6">
					<img src="/logos/djangorest.svg" alt="Django Rest Logo" className="h-12 w-12 object-contain" />
					<img src="/logos/tailwindcss.svg" alt="Tailwind CSS Logo" className="h-12 w-12 object-contain" />
					<img src="/logos/typescript.svg" alt="TypeScript Logo" className="h-12 w-12 object-contain" />
					<img src="/logos/vite.svg" alt="Vite Logo" className="h-12 w-12 object-contain" />
					<img src="/logos/postgresql.svg" alt="PostgreSql Logo" className="h-12 w-12 object-contain" />
				</div>
			</div>
			
			{/* Private Route Test */}
			<Link to="/testprivateroute" className="mt-6">
				<button className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition">Test Private Route Here!</button>
			</Link>
		</div>
	);
}

export default Landing;
